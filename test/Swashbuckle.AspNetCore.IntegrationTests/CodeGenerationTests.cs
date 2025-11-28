#if NET10_0_OR_GREATER
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;
using Microsoft.Build.Utilities.ProjectCreation;
using NSwag.CodeGeneration.CSharp;
using Swashbuckle.AspNetCore.TestSupport.Utilities;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData("Kiota", "https://petstore3.swagger.io/api/v3/openapi.json")]
    [InlineData("NSwag", "https://petstore3.swagger.io/api/v3/openapi.json")]
    public async Task OpenApiDocument_Generates_Valid_Client_Code(string generator, string openApiDocumentUrl)
    {
        // Arrange
        using var project = await GenerateClientAsync(generator, openApiDocumentUrl);

        // Act and Assert
        await CompileClientAsync(project.Path);
    }

    private async Task CompileClientAsync(string location)
    {
        var startInfo = new ProcessStartInfo(
            "dotnet",
            [
                "build",
                location,
                "--configuration",
                "Release",
            ])
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        using var process = Process.Start(startInfo);

        var readOutput = ReadOutputAsync(process, TestContext.Current.CancellationToken);

        await process.WaitForExitAsync();

        (string error, string output) = await readOutput;

        outputHelper.WriteLine(output);
        outputHelper.WriteLine(error);

        Assert.Equal(0, process.ExitCode);
    }

    private async Task<TemporaryDirectory> GenerateClientAsync(string generator, string url)
    {
        TemporaryDirectory project;

        switch (generator.ToLowerInvariant())
        {
            case "kiota":
                project = await GenerateProjectAsync(["Microsoft.Kiota.Bundle"]);
                await GenerateClientWithKiotaAsync(project.Path, url);
                break;

            case "nswag":
                project = await GenerateProjectAsync(["Newtonsoft.Json"]);
                await GenerateClientWithNSwagAsync(project.Path, url);
                break;

            default:
                throw new NotSupportedException($"The generator '{generator}' is not supported.");
        }

        return project;
    }

    private async Task GenerateClientWithKiotaAsync(string outputPath, string url)
    {
        // https://learn.microsoft.com/openapi/kiota/using#client-generation
        var startInfo = new ProcessStartInfo(
            "dotnet",
            [
                "kiota",
                "generate",
                "--class-name", "KiotaOpenApiClient",
                "--disable-ssl-validation",
                ////"--disable-validation-rules", "all",
                "--language", "csharp",
                "--namespace-name", "Swashbuckle.AspNetCore.IntegrationTests.KiotaTests",
                "--openapi", url,
                "--output", outputPath,
            ])
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
        };

        // https://learn.microsoft.com/openapi/kiota/using#configuration-environment-variables
        startInfo.Environment.Add("KIOTA_TUTORIAL_ENABLED", "false");

        using var process = Process.Start(startInfo);

        var readOutput = ReadOutputAsync(process, TestContext.Current.CancellationToken);

        await process.WaitForExitAsync();

        (string error, string output) = await readOutput;

        outputHelper.WriteLine(output);
        outputHelper.WriteLine(error);

        Assert.Equal(0, process.ExitCode);
    }

    private static async Task GenerateClientWithNSwagAsync(string outputPath, string url)
    {
        // https://github.com/RicoSuter/NSwag/wiki/CSharpClientGenerator
        var document = await NSwag.OpenApiDocument.FromUrlAsync(url);

        var settings = new CSharpClientGeneratorSettings
        {
            ClassName = "NSwagOpenApiClient",
            CSharpGeneratorSettings = { Namespace = "Swashbuckle.AspNetCore.IntegrationTests.NSwagTests" },
        };

        var generator = new CSharpClientGenerator(document, settings);
        string code = generator.GenerateFile();

        await File.WriteAllTextAsync(Path.Combine(outputPath, "NSwagOpenApiClient.cs"), code);
    }

    private static async Task<TemporaryDirectory> GenerateProjectAsync(params string[] packageReferences)
    {
        var directory = new TemporaryDirectory();

        var project = ProjectCreator
            .Create(sdk: ProjectCreatorConstants.SdkCsprojDefaultSdk)
            .Property("EnableNETAnalyzers", "false")
            .Property("EnforceCodeStyleInBuild", "false")
            .Property("GenerateDocumentationFile", "false")
            .Property("LangVersion", "latest")
            .Property("NoWarn", "$(NoWarn)")
            .Property("OutputType", "Library")
            .Property("TargetFramework", "net10.0")
            .Property("TreatWarningsAsErrors", "false");

        foreach (var name in packageReferences)
        {
            project.ItemPackageReference(name, GetNuGetPackageVersion(name));
        }

        await File.WriteAllTextAsync(
            Path.Combine(directory.Path, "GeneratedOpenApiClient.csproj"),
            project.Xml);

        await File.WriteAllTextAsync(
            Path.Combine(directory.Path, "global.json"),
            CreateGlobalJson(await GetDotNetSdkVersion()));

        return directory;
    }

    private static string CreateGlobalJson(string sdkVersion)
    {
        var globalJson = new JsonObject()
        {
            ["sdk"] = new JsonObject()
            {
                ["version"] = sdkVersion,
            },
        };

        return globalJson.ToJsonString(new(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            WriteIndented = true,
        });
    }

    private static async Task<string> GetDotNetSdkVersion()
    {
        var solutionRoot = typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "SolutionRoot")
            .Value!;

        var path = Path.Combine(solutionRoot, "global.json");

        using var json = File.OpenRead(path);
        using var document = await JsonDocument.ParseAsync(json);

        return document.RootElement
            .GetProperty("sdk")
            .GetProperty("version")
            .GetString();
    }

    private static string GetNuGetPackageVersion(string name)
    {
        var solutionRoot = typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "SolutionRoot")
            .Value!;

        var path = Path.Combine(solutionRoot, "Directory.Packages.props");
        var xml = File.ReadAllText(path);

        var project = XDocument.Parse(xml);

        Assert.NotNull(project.Root);
        var ns = project.Root.GetDefaultNamespace();

        var version = project
            .Root?
            .Elements(ns + "ItemGroup")
            .Elements(ns + "PackageVersion")
            .Select((p) =>
                new
                {
                    Key = p.Attribute("Include")?.Value ?? string.Empty,
                    Value = p.Attribute("Version")?.Value ?? p.Element(ns + "Version")?.Value ?? string.Empty,
                })
            .Where((p) => p.Key == name)
            .Select((p) => p.Value)
            .FirstOrDefault();

        return version ?? throw new InvalidOperationException($"Failed to get version for package {name}.");
    }

    private static async Task<(string Error, string Output)> ReadOutputAsync(
            Process process,
            CancellationToken cancellationToken)
    {
        var processErrors = ConsumeStreamAsync(process.StandardError, process.StartInfo.RedirectStandardError, cancellationToken);
        var processOutput = ConsumeStreamAsync(process.StandardOutput, process.StartInfo.RedirectStandardOutput, cancellationToken);

        await Task.WhenAll(processErrors, processOutput);

        string error = string.Empty;
        string output = string.Empty;

        if (processErrors.Status == TaskStatus.RanToCompletion)
        {
            error = (await processErrors).ToString();
        }

        if (processOutput.Status == TaskStatus.RanToCompletion)
        {
            output = (await processOutput).ToString();
        }

        return (error, output);
    }

    private static Task<StringBuilder> ConsumeStreamAsync(
        StreamReader reader,
        bool isRedirected,
        CancellationToken cancellationToken)
    {
        return isRedirected ?
            Task.Run(() => ProcessStream(reader, cancellationToken), cancellationToken) :
            Task.FromResult(new StringBuilder(0));

        static async Task<StringBuilder> ProcessStream(
            StreamReader reader,
            CancellationToken cancellationToken)
        {
            var builder = new StringBuilder();

            try
            {
                builder.Append(await reader.ReadToEndAsync(cancellationToken));
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }

            return builder;
        }
    }
}
#endif
