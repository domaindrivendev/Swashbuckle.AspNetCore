using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test;

[CollectionDefinition(nameof(ConsoleErrorCollection), DisableParallelization = true)]
public sealed class ConsoleErrorCollection;

public sealed class ErrorHandlingTests
{
    private static readonly ManualResetEventSlim HostResolverTimeoutGate = new(false);
    private static readonly ManualResetEventSlim PreBuildTimeoutGate = new(false);
    private static readonly ManualResetEventSlim PostBuildTimeoutGate = new(false);
    private static readonly ManualResetEventSlim ProgramTimeoutGate = new(false);

    [Theory]
    [InlineData(30d, "30 seconds")]
    [InlineData(1.5d, "1.5 seconds")]
    [InlineData(0.25d, "250 ms")]
    public void FormatTimeout_Returns_Expected_Invariant_String(double seconds, string expected)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            var actual = HostFactoryResolver.FormatTimeout(TimeSpan.FromSeconds(seconds));

            Assert.Equal(expected, actual);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void HostFactoryResolver_Throws_InvalidOperationException_When_EntryPoint_Exits_Without_Building_Host()
    {
        var assembly = CreateEntryPointAssembly(nameof(NoOpEntryPoint));
        var factory = HostFactoryResolver.ResolveHostFactory(assembly, waitTimeout: TimeSpan.FromSeconds(5));

        var ex = Assert.Throws<InvalidOperationException>(() => factory([]));

        Assert.Contains("exited without building an IHost", ex.Message, StringComparison.Ordinal);
        Assert.Contains(typeof(ErrorHandlingTests).Assembly.GetName().Name, ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void HostFactoryResolver_Throws_TimeoutException_When_EntryPoint_Blocks_Before_Build()
    {
        var assembly = CreateEntryPointAssembly(nameof(HostResolverBlockingEntryPoint));
        var factory = HostFactoryResolver.ResolveHostFactory(assembly, waitTimeout: TimeSpan.FromMilliseconds(50));

        HostResolverTimeoutGate.Reset();
        try
        {
            var ex = Assert.Throws<TimeoutException>(() => factory([]));

            Assert.Contains("50 ms", ex.Message, StringComparison.Ordinal);
            Assert.Contains("to build an IHost", ex.Message, StringComparison.Ordinal);
            Assert.Contains("SwaggerHostFactory", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            HostResolverTimeoutGate.Set();
        }
    }

    [Fact]
    public void HostingApplication_Returns_Null_When_Assembly_Has_No_EntryPoint()
    {
        var assembly = new TestEntryPointAssembly(entryPoint: null);

        var serviceProvider = HostingApplication.GetServiceProvider(assembly, TimeSpan.FromMilliseconds(50));

        Assert.Null(serviceProvider);
    }

    [Fact]
    public void HostingApplication_Propagates_PreBuild_EntryPoint_Exception()
    {
        var assembly = CreateEntryPointAssembly(nameof(PreBuildThrowingEntryPoint));

        var ex = Assert.Throws<InvalidOperationException>(
            () => HostingApplication.GetServiceProvider(assembly, TimeSpan.FromSeconds(5)));

        Assert.Equal("Pre-build startup failure", ex.Message);
    }

    [Fact]
    public void HostingApplication_Propagates_PreBuild_TimeoutException()
    {
        var assembly = CreateEntryPointAssembly(nameof(PreBuildBlockingEntryPoint));

        PreBuildTimeoutGate.Reset();
        try
        {
            var ex = Assert.Throws<TimeoutException>(
                () => HostingApplication.GetServiceProvider(assembly, TimeSpan.FromMilliseconds(50)));

            Assert.Contains("50 ms", ex.Message, StringComparison.Ordinal);
            Assert.Contains("to build an IHost", ex.Message, StringComparison.Ordinal);
            Assert.Contains("SwaggerHostFactory", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            PreBuildTimeoutGate.Set();
        }
    }

    [Fact]
    public void HostingApplication_Throws_TimeoutException_When_EntryPoint_Blocks_After_Build_Before_Start()
    {
        var assembly = CreateEntryPointAssembly(nameof(PostBuildBlockingEntryPoint));

        PostBuildTimeoutGate.Reset();
        try
        {
            var ex = Assert.Throws<TimeoutException>(
                () => HostingApplication.GetServiceProvider(assembly, TimeSpan.FromMilliseconds(100)));

            Assert.Contains("100 ms", ex.Message, StringComparison.Ordinal);
            Assert.Contains(assembly.GetName().Name, ex.Message, StringComparison.Ordinal);
            Assert.Contains("to start the application", ex.Message, StringComparison.Ordinal);
            Assert.Contains("SwaggerHostFactory", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            PostBuildTimeoutGate.Set();
        }
    }

    [Fact]
    public void HostingApplication_Propagates_Unwrapped_PostBuild_EntryPoint_Exception()
    {
        var assembly = CreateEntryPointAssembly(nameof(PostBuildThrowingEntryPoint));

        var ex = Assert.Throws<InvalidOperationException>(
            () => HostingApplication.GetServiceProvider(assembly, TimeSpan.FromSeconds(5)));

        Assert.Equal("Post-build startup failure", ex.Message);
    }

    [Fact]
    public void HostingApplication_Returns_ServiceProvider_When_EntryPoint_Exits_After_Build()
    {
        var assembly = CreateEntryPointAssembly(nameof(PostBuildExitingEntryPoint));

        var serviceProvider = HostingApplication.GetServiceProvider(assembly, TimeSpan.FromSeconds(5));

        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void Program_GetServiceProvider_Does_Not_Mask_Timeout_With_Startup_Not_Found()
    {
        var assembly = CreateEntryPointAssembly(nameof(ProgramBlockingEntryPoint));

        ProgramTimeoutGate.Reset();
        try
        {
            var ex = Assert.Throws<TimeoutException>(
                () => Program.GetServiceProvider(assembly, TimeSpan.FromMilliseconds(50)));

            Assert.DoesNotContain("Startup", ex.Message, StringComparison.Ordinal);
            Assert.Contains("SwaggerHostFactory", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            ProgramTimeoutGate.Set();
        }
    }

    [Fact]
    public void Program_GetServiceProvider_Rethrows_Startup_Exception_When_No_EntryPoint()
    {
        var assembly = new TestEntryPointAssembly(entryPoint: null);

        var ex = Assert.Throws<InvalidOperationException>(
            () => Program.GetServiceProvider(assembly, TimeSpan.FromMilliseconds(50)));

        Assert.Contains("Startup", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_GetServiceProvider_Unwraps_TargetInvocationException_From_SwaggerHostFactory()
    {
        var assembly = new TestEntryPointAssembly(
            entryPoint: null,
            definedTypes: [typeof(ThrowingHostFactoryContainer.SwaggerHostFactory).GetTypeInfo()]);

        var ex = Assert.Throws<InvalidOperationException>(() => Program.GetServiceProvider(assembly));

        Assert.Equal("Custom host factory failure", ex.Message);
    }

    [Fact]
    public async Task ToFile_SubProcess_Writes_Error_To_StandardError_And_Returns_One()
    {
        var testAssemblyLocation = typeof(ErrorHandlingTests).Assembly.Location;
        var depsFile = Path.ChangeExtension(testAssemblyLocation, ".deps.json");
        var runtimeConfig = Path.ChangeExtension(testAssemblyLocation, ".runtimeconfig.json");
        var cliAssemblyLocation = typeof(Program).Assembly.Location;
        var startupAssembly = Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll");

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            }
        };

        process.StartInfo.ArgumentList.Add("exec");
        process.StartInfo.ArgumentList.Add("--depsfile");
        process.StartInfo.ArgumentList.Add(depsFile);
        process.StartInfo.ArgumentList.Add("--runtimeconfig");
        process.StartInfo.ArgumentList.Add(runtimeConfig);
        process.StartInfo.ArgumentList.Add(cliAssemblyLocation);
        process.StartInfo.ArgumentList.Add("tofile");
        process.StartInfo.ArgumentList.Add(startupAssembly);
        process.StartInfo.ArgumentList.Add("nonexistent");

        Assert.True(process.Start());

        var cancellationToken = TestContext.Current.CancellationToken;
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        string stdout = await stdoutTask;
        string stderr = await stderrTask;

        Assert.Equal(1, process.ExitCode);
        Assert.Contains("nonexistent", stderr, StringComparison.Ordinal);
        Assert.DoesNotContain("Unhandled exception.", stderr, StringComparison.Ordinal);
        Assert.Equal(string.Empty, stdout);
    }

    private static TestEntryPointAssembly CreateEntryPointAssembly(string methodName)
    {
        var method = typeof(ErrorHandlingTests).GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        return new TestEntryPointAssembly(method);
    }

    private static void NoOpEntryPoint()
    {
    }

    private static void PreBuildThrowingEntryPoint()
        => throw new InvalidOperationException("Pre-build startup failure");

    private static void HostResolverBlockingEntryPoint()
        => HostResolverTimeoutGate.Wait(TimeSpan.FromSeconds(10));

    private static void PreBuildBlockingEntryPoint()
        => PreBuildTimeoutGate.Wait(TimeSpan.FromSeconds(10));

    private static void ProgramBlockingEntryPoint()
        => ProgramTimeoutGate.Wait(TimeSpan.FromSeconds(10));

    private static void PostBuildBlockingEntryPoint(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        using var app = builder.Build();
        PostBuildTimeoutGate.Wait(TimeSpan.FromSeconds(10));
    }

    private static void PostBuildThrowingEntryPoint(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        using var app = builder.Build();
        throw new InvalidOperationException("Post-build startup failure");
    }

    private static void PostBuildExitingEntryPoint(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        _ = builder.Build();
    }

    private static class ThrowingHostFactoryContainer
    {
        public static class SwaggerHostFactory
        {
            public static IHost CreateHost()
                => throw new InvalidOperationException("Custom host factory failure");
        }
    }

    private sealed class TestEntryPointAssembly(MethodInfo entryPoint, IEnumerable<TypeInfo> definedTypes = null) : Assembly
    {
        private readonly IEnumerable<TypeInfo> _definedTypes = definedTypes ?? [];

        public override MethodInfo EntryPoint => entryPoint;

        public override IEnumerable<TypeInfo> DefinedTypes => _definedTypes;

        public override AssemblyName GetName() => typeof(ErrorHandlingTests).Assembly.GetName();

        public override AssemblyName GetName(bool copiedName) => GetName();
    }
}

[Collection(nameof(ConsoleErrorCollection))]
public sealed class ConsoleErrorTests
{
    [Fact]
    public async Task Main_Internal_ToFile_Writes_Error_To_ConsoleError_And_Returns_One()
    {
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            string[] args =
            [
                "_tofile",
                Path.Combine(Directory.GetCurrentDirectory(), "Basic.dll"),
                "nonexistent"
            ];

            int exitCode = await Program.Main(args);

            Assert.Equal(1, exitCode);
            Assert.Contains("nonexistent", errorWriter.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    [Fact]
    public async Task Main_Internal_List_Writes_Error_To_ConsoleError_And_Returns_One()
    {
        var originalError = Console.Error;
        using var errorWriter = new StringWriter();
        Console.SetError(errorWriter);

        try
        {
            string[] args =
            [
                "_list",
                Path.Combine(Directory.GetCurrentDirectory(), "does_not_exist.dll")
            ];

            int exitCode = await Program.Main(args);

            Assert.Equal(1, exitCode);
            Assert.Contains("does_not_exist.dll", errorWriter.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }
}
