#if NET10_0_OR_GREATER

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Xml.Linq;
using Microsoft.Build.Utilities.ProjectCreation;
using Microsoft.OpenApi;
using NSwag.CodeGeneration.CSharp;
using Swashbuckle.AspNetCore.TestSupport.Utilities;

namespace Swashbuckle.AspNetCore.IntegrationTests;

internal sealed class ClientGenerator(ITestOutputHelper outputHelper)
{
    public static bool IsSupported(ClientGeneratorTool generator, string format, OpenApiSpecVersion version) => generator switch
    {
        ClientGeneratorTool.Kiota => format switch
        {
            "json" or "yaml" => version switch
            {
                OpenApiSpecVersion.OpenApi2_0 => true,
                OpenApiSpecVersion.OpenApi3_0 => true,
                OpenApiSpecVersion.OpenApi3_1 => true,
                _ => false,
            },
            _ => false,
        },
        ClientGeneratorTool.NSwag => format switch
        {
            "json" => version switch
            {
                OpenApiSpecVersion.OpenApi2_0 => true,
                OpenApiSpecVersion.OpenApi3_0 => true,
                _ => false,
            },
            _ => false,
        },
        _ => false,
    };

    public async Task CompileAsync(string location)
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

    public async Task<TemporaryDirectory> GenerateFromStringAsync(ClientGeneratorTool generator, string openApiDocument)
    {
        TemporaryDirectory project;

        switch (generator)
        {
            case ClientGeneratorTool.Kiota:
                project = await GenerateProjectAsync(["Microsoft.Kiota.Bundle"]);
                await GenerateClientFromStringWithKiotaAsync(project.Path, openApiDocument, outputHelper);
                break;

            case ClientGeneratorTool.NSwag:
                project = await GenerateProjectAsync(["Newtonsoft.Json"]);
                await GenerateClientFromStringWithNSwagAsync(project.Path, openApiDocument);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(generator), generator, $"The client generator tool '{generator}' is not supported.");
        }

        return project;
    }

    public async Task<TemporaryDirectory> GenerateFromUrlAsync(ClientGeneratorTool generator, string openApiDocumentUrl)
    {
        TemporaryDirectory project;

        switch (generator)
        {
            case ClientGeneratorTool.Kiota:
                project = await GenerateProjectAsync(["Microsoft.Kiota.Bundle"]);
                await GenerateClientFromUrlWithKiotaAsync(project.Path, openApiDocumentUrl, outputHelper);
                break;

            case ClientGeneratorTool.NSwag:
                project = await GenerateProjectAsync(["Newtonsoft.Json"]);
                await GenerateClientFromUrlWithNSwagAsync(project.Path, openApiDocumentUrl);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(generator), generator, $"The client generator tool '{generator}' is not supported.");
        }

        return project;
    }

    private static async Task GenerateClientFromStringWithKiotaAsync(string outputPath, string content, ITestOutputHelper outputHelper)
    {
        string tempFile = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFile, content);

        try
        {
            await GenerateClientFromUrlWithKiotaAsync(outputPath, tempFile, outputHelper);
        }
        finally
        {
            try
            {
                File.Delete(tempFile);
            }
            catch (Exception)
            {
                // Ignore
            }
        }
    }

    private static async Task GenerateClientFromUrlWithKiotaAsync(string outputPath, string url, ITestOutputHelper outputHelper)
    {
        // https://learn.microsoft.com/openapi/kiota/using#client-generation
        var startInfo = new ProcessStartInfo(
            "dotnet",
            [
                "kiota",
                "generate",
                "--class-name", "KiotaOpenApiClient",
                "--disable-ssl-validation",
                "--disable-validation-rules", "all",
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

    private static async Task GenerateClientFromStringWithNSwagAsync(string outputPath, string content)
    {
        var document = await NSwag.OpenApiDocument.FromJsonAsync(content);
        await GenerateClientFromUrlWithNSwagAsync(outputPath, document);
    }

    private static async Task GenerateClientFromUrlWithNSwagAsync(string outputPath, string url)
    {
        var document = await NSwag.OpenApiDocument.FromUrlAsync(url);
        await GenerateClientFromUrlWithNSwagAsync(outputPath, document);
    }

    private static async Task GenerateClientFromUrlWithNSwagAsync(string outputPath, NSwag.OpenApiDocument document)
    {
        // https://github.com/RicoSuter/NSwag/wiki/CSharpClientGenerator
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
            .Property("Nullable", "enable")
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

    private static string GetSolutionRoot() =>
        typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "SolutionRoot")
            .Value!;

    private static async Task<string> GetDotNetSdkVersion()
    {
        var path = Path.Combine(GetSolutionRoot(), "global.json");

        using var json = File.OpenRead(path);
        using var document = await JsonDocument.ParseAsync(json);

        return document.RootElement
            .GetProperty("sdk")
            .GetProperty("version")
            .GetString();
    }

    private static string GetNuGetPackageVersion(string name)
    {
        var path = Path.Combine(GetSolutionRoot(), "Directory.Packages.props");
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
