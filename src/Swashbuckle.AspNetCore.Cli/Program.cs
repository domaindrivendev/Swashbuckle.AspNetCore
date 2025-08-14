using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.Cli;

internal class Program
{
    private const string OpenApiVersionOption = "--openapiversion";

    public static int Main(string[] args)
    {
        // Helper to simplify command line parsing etc.
        var runner = new CommandRunner("dotnet swagger", "Swashbuckle (Swagger) Command Line Tools", Console.Out);

        // NOTE: The "dotnet swagger tofile" command does not serve the request directly. Instead, it invokes a corresponding
        // command (called _tofile) via "dotnet exec" so that the runtime configuration (*.runtimeconfig & *.deps.json) of the
        // provided startupassembly can be used instead of the tool's. This is neccessary to successfully load the
        // startupassembly and it's transitive dependencies. See https://github.com/dotnet/coreclr/issues/13277 for more.

        // > dotnet swagger tofile ...
        runner.SubCommand("tofile", "retrieves Swagger from a startup assembly, and writes to file", c =>
        {
            c.Argument("startupassembly", "relative path to the application's startup assembly");
            c.Argument("swaggerdoc", "name of the swagger doc you want to retrieve, as configured in your startup class");

            c.Option("--output", "relative path where the Swagger will be output, defaults to stdout");
            c.Option("--host", "a specific host to include in the Swagger output");
            c.Option("--basepath", "a specific basePath to include in the Swagger output");
            c.Option(OpenApiVersionOption, "output Swagger in the specified version, defaults to 3.0");
            c.Option("--yaml", "exports swagger in a yaml format", true);

            c.OnRun((namedArgs) =>
            {
                string subProcessCommandLine = PrepareCommandLine(args, namedArgs);

                using var child = Process.Start("dotnet", subProcessCommandLine);

                child.WaitForExit();
                return child.ExitCode;
            });
        });

        // > dotnet swagger _tofile ... (* should only be invoked via "dotnet exec")
        runner.SubCommand("_tofile", "", c =>
        {
            c.Argument("startupassembly", "");
            c.Argument("swaggerdoc", "");
            c.Option("--output", "");
            c.Option("--host", "");
            c.Option("--basepath", "");
            c.Option(OpenApiVersionOption, "");
            c.Option("--yaml", "", true);

            c.OnRun((namedArgs) =>
            {
                SetupAndRetrieveSwaggerProviderAndOptions(namedArgs, out var swaggerProvider, out var swaggerOptions);
                var swaggerDocumentSerializer = swaggerOptions?.Value?.CustomDocumentSerializer;
                var swagger = swaggerProvider.GetSwagger(
                    namedArgs["swaggerdoc"],
                    namedArgs.TryGetValue("--host", out var arg) ? arg : null,
                    namedArgs.TryGetValue("--basepath", out var namedArg) ? namedArg : null);

                // 4) Serialize to specified output location or stdout
                var outputPath = namedArgs.TryGetValue("--output", out var arg1)
                    ? Path.Combine(Directory.GetCurrentDirectory(), arg1)
                    : null;

                if (!string.IsNullOrEmpty(outputPath))
                {
                    string directoryPath = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }

                using Stream stream = outputPath != null ? File.Create(outputPath) : Console.OpenStandardOutput();
                using var streamWriter = new FormattingStreamWriter(stream, CultureInfo.InvariantCulture);

                IOpenApiWriter writer;
                if (namedArgs.ContainsKey("--yaml"))
                {
                    writer = new OpenApiYamlWriter(streamWriter);
                }
                else
                {
                    writer = new OpenApiJsonWriter(streamWriter);
                }

                OpenApiSpecVersion specVersion = OpenApiSpecVersion.OpenApi3_0;

                if (namedArgs.TryGetValue(OpenApiVersionOption, out var versionArg))
                {
                    specVersion = versionArg switch
                    {
                        "2.0" => OpenApiSpecVersion.OpenApi2_0,
                        "3.0" => OpenApiSpecVersion.OpenApi3_0,
                        "3.1" => OpenApiSpecVersion.OpenApi3_1,
                        _ => throw new NotSupportedException($"The specified OpenAPI version \"{versionArg}\" is not supported."),
                    };
                }

                if (swaggerDocumentSerializer != null)
                {
                    swaggerDocumentSerializer.SerializeDocument(swagger, writer, specVersion);
                }
                else
                {
                    swagger.SerializeAs(specVersion, writer);
                }

                if (outputPath != null)
                {
                    Console.WriteLine($"Swagger JSON/YAML successfully written to {outputPath}");
                }

                return 0;
            });
        });

        // > dotnet swagger list
        runner.SubCommand("list", "retrieves the list of Swagger document names from a startup assembly", c =>
        {
            c.Argument("startupassembly", "relative path to the application's startup assembly");
            c.Option("--output", "relative path where the document names will be output, defaults to stdout");
            c.OnRun((namedArgs) =>
            {
                string subProcessCommandLine = PrepareCommandLine(args, namedArgs);

                using var child = Process.Start("dotnet", subProcessCommandLine);

                child.WaitForExit();
                return child.ExitCode;
            });
        });

        // > dotnet swagger _list ... (* should only be invoked via "dotnet exec")
        runner.SubCommand("_list", "", c =>
        {
            c.Argument("startupassembly", "");
            c.Option("--output", "");
            c.OnRun((namedArgs) =>
            {
                SetupAndRetrieveSwaggerProviderAndOptions(namedArgs, out var swaggerProvider, out var swaggerOptions);
                IList<string> docNames = [];

                string outputPath = namedArgs.TryGetValue("--output", out var arg1)
                    ? Path.Combine(Directory.GetCurrentDirectory(), arg1)
                    : null;
                bool outputViaConsole = outputPath == null;
                if (!string.IsNullOrEmpty(outputPath))
                {
                    string directoryPath = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                }

                using Stream stream = outputViaConsole ? Console.OpenStandardOutput() : File.Create(outputPath);
                using StreamWriter writer = new(stream, outputViaConsole ? Console.OutputEncoding : Encoding.UTF8);

                if (swaggerProvider is not ISwaggerDocumentMetadataProvider docMetaProvider)
                {
                    writer.WriteLine($"The registered {nameof(ISwaggerProvider)} instance does not implement {nameof(ISwaggerDocumentMetadataProvider)}; unable to list the Swagger document names.");
                    return -1;
                }

                docNames = docMetaProvider.GetDocumentNames();

                foreach (var name in docNames)
                {
                    writer.WriteLine($"\"{name}\"");
                }

                return 0;
            });
        });

        return runner.Run(args);
    }

    private static void SetupAndRetrieveSwaggerProviderAndOptions(IDictionary<string, string> namedArgs, out ISwaggerProvider swaggerProvider, out IOptions<SwaggerOptions> swaggerOptions)
    {
        // 1) Configure host with provided startupassembly
        var startupAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
            Path.Combine(Directory.GetCurrentDirectory(), namedArgs["startupassembly"]));

        // 2) Build a service container that's based on the startup assembly
        var serviceProvider = GetServiceProvider(startupAssembly);

        // 3) Retrieve Swagger via configured provider
        swaggerProvider = serviceProvider.GetRequiredService<ISwaggerProvider>();
        swaggerOptions = serviceProvider.GetService<IOptions<SwaggerOptions>>();
    }

    private static string PrepareCommandLine(string[] args, IDictionary<string, string> namedArgs)
    {
        if (!File.Exists(namedArgs["startupassembly"]))
        {
            throw new FileNotFoundException(namedArgs["startupassembly"]);
        }

        var depsFile = namedArgs["startupassembly"].Replace(".dll", ".deps.json");
        var runtimeConfig = namedArgs["startupassembly"].Replace(".dll", ".runtimeconfig.json");
        var commandName = args[0];

        var subProcessArguments = new string[args.Length - 1];
        if (subProcessArguments.Length > 0)
        {
            Array.Copy(args, 1, subProcessArguments, 0, subProcessArguments.Length);
        }

        var subProcessCommandLine = string.Format(
            "exec --depsfile {0} --runtimeconfig {1} {2} _{3} {4}", // note the underscore prepended to the command name
            EscapePath(depsFile),
            EscapePath(runtimeConfig),
            EscapePath(typeof(Program).Assembly.Location),
            commandName,
            string.Join(" ", subProcessArguments.Select(EscapePath))
        );
        return subProcessCommandLine;
    }

    private static string EscapePath(string path)
    {
        return path.Contains(' ')
            ? "\"" + path + "\""
            : path;
    }

    private static IServiceProvider GetServiceProvider(Assembly startupAssembly)
    {
        if (TryGetCustomHost(startupAssembly, "SwaggerHostFactory", "CreateHost", out IHost host))
        {
            return host.Services;
        }

        if (TryGetCustomHost(startupAssembly, "SwaggerWebHostFactory", "CreateWebHost", out IWebHost webHost))
        {
            return webHost.Services;
        }

        try
        {
            return Host.CreateDefaultBuilder()
               .ConfigureWebHostDefaults(builder => builder.UseStartup(startupAssembly.GetName().Name))
               .Build()
               .Services;
        }
        catch
        {
            var serviceProvider = HostingApplication.GetServiceProvider(startupAssembly);

            if (serviceProvider != null)
            {
                return serviceProvider;
            }

            throw;
        }
    }

    private static bool TryGetCustomHost<THost>(
        Assembly startupAssembly,
        string factoryClassName,
        string factoryMethodName,
        out THost host)
    {
        // Scan the assembly for any types that match the provided naming convention
        var factoryTypes = startupAssembly.DefinedTypes
            .Where(t => t.Name == factoryClassName)
            .ToList();

        if (factoryTypes.Count == 0)
        {
            host = default;
            return false;
        }
        else if (factoryTypes.Count > 1)
        {
            throw new InvalidOperationException($"Multiple {factoryClassName} classes detected");
        }

        var factoryMethod = factoryTypes
            .Single()
            .GetMethod(factoryMethodName, BindingFlags.Public | BindingFlags.Static);

        if (factoryMethod == null || factoryMethod.ReturnType != typeof(THost))
        {
            throw new InvalidOperationException(
                $"{factoryClassName} class detected but does not contain a public static method " +
                $"called {factoryMethodName} with return type {typeof(THost).Name}");
        }

        host = (THost)factoryMethod.Invoke(null, null);
        return true;
    }
}
