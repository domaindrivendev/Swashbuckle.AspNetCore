#if NET10_0_OR_GREATER

using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<ClientGeneratorTool, Type, string, bool> ApplicationTestCases()
    {
        var documents = new[]
        {
            // Startup-based applications
            (typeof(Basic.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(GenericControllers.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(ReDocApp.Startup), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, false),
            (typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json", OpenApiSpecVersion.OpenApi3_0, false),
            // Minimal API-based applications
            (typeof(MinimalApp.Program), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, true),
            (typeof(MinimalAppWithNullableEnums.Program), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, true),
            (typeof(MvcWithNullable.Program), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, true),
            (typeof(TopLevelSwaggerDoc.Program), "/swagger/v1.json", OpenApiSpecVersion.OpenApi3_0, true),
            (typeof(WebApi.Program), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, true),
            (typeof(WebApi.Aot.Program), "/swagger/v1/swagger.json", OpenApiSpecVersion.OpenApi3_0, true),
        };

        var testCases = new TheoryData<ClientGeneratorTool, Type, string, bool>();

        foreach (var tool in Enum.GetValues<ClientGeneratorTool>())
        {
            foreach ((var type, var url, var version, var isMinimal) in documents)
            {
                if (tool is ClientGeneratorTool.NSwag && type == typeof(Basic.Startup))
                {
                    // NSwag doesn't generate valid compilation due to a missing FileResponse type
                    continue;
                }

                if (ClientGenerator.IsSupported(tool, "json", version))
                {
                    testCases.Add(tool, type, url, isMinimal);
                }
            }
        }

        return testCases;
    }

    [Theory]
    [MemberData(nameof(ApplicationTestCases))]
    public async Task OpenApiDocument_Generates_Valid_Client_Code_From_Application(
        ClientGeneratorTool tool,
        Type startupType,
        string openApiDocumentUrl,
        bool isMinimal)
    {
        // Arrange
        var generator = new ClientGenerator(outputHelper);

        using var client = GetHttpClientForApplication(startupType, isMinimal);
        var document = await GetOpenApiDocumentAsync(client, openApiDocumentUrl);

        using var project = await generator.GenerateFromStringAsync(tool, document);

        // Act and Assert
        await generator.CompileAsync(project.Path);
    }

    public static TheoryData<ClientGeneratorTool, string> SnapshotTestCases()
    {
        var testCases = new TheoryData<ClientGeneratorTool, string>();
        
        foreach (var path in Directory.EnumerateFiles(Path.Combine(GetProjectRoot(), "snapshots"), "*.txt", SearchOption.AllDirectories))
        {
            // Deduplicate by ignoring snapshots for other TFMs
            if (!path.EndsWith(".DotNet10_0.verified.txt", StringComparison.Ordinal))
            {
                continue;
            }

            using var snapshot = File.OpenRead(path);
            using var document = JsonDocument.Parse(snapshot);

            if (!document.RootElement.TryGetProperty("openapi", out var property) &&
                !document.RootElement.TryGetProperty("swagger", out property))
            {
                continue;
            }

            if (!Version.TryParse(property.GetString(), out var documentVersion))
            {
                continue;
            }

            var version = documentVersion switch
            {
                { Major: 2 } => OpenApiSpecVersion.OpenApi2_0,
                { Major: 3, Minor: 0 } => OpenApiSpecVersion.OpenApi3_0,
                { Major: 3, Minor: 1 } => OpenApiSpecVersion.OpenApi3_1,
                _ => throw new NotSupportedException(path),
            };

            foreach (var tool in Enum.GetValues<ClientGeneratorTool>())
            {
                if (tool is ClientGeneratorTool.NSwag && Path.GetFileNameWithoutExtension(path).Contains("Basic.Startup"))
                {
                    // NSwag doesn't generate valid compilation due to a missing FileResponse type
                    continue;
                }

                if (ClientGenerator.IsSupported(tool, "json", version))
                {
                    testCases.Add(tool, path);
                }
            }
        }

        return testCases;
    }

    [Theory]
    [MemberData(nameof(SnapshotTestCases))]
    public async Task OpenApiDocument_Generates_Valid_Client_Code_From_Snapshot(ClientGeneratorTool tool, string path)
    {
        // Arrange
        var generator = new ClientGenerator(outputHelper);
        var document = await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken);

        using var project = await generator.GenerateFromStringAsync(tool, document);

        // Act and Assert
        await generator.CompileAsync(project.Path);
    }

    private static string GetProjectRoot() =>
        typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "ProjectRoot")
            .Value!;

    private static async Task<string> GetOpenApiDocumentAsync(HttpClient client, string url)
    {
        using var swaggerResponse = await client.GetAsync(url);

        Assert.True(swaggerResponse.IsSuccessStatusCode, $"IsSuccessStatusCode is false. Response: '{await swaggerResponse.Content.ReadAsStringAsync()}'");

        using var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();

        string document;

        using (var reader = new StreamReader(contentStream, leaveOpen: true))
        {
            document = await reader.ReadToEndAsync();
        }

        contentStream.Seek(0, SeekOrigin.Begin);
        var (_, diagnostic) = await OpenApiDocumentLoader.LoadWithDiagnosticsAsync(contentStream);

        Assert.NotNull(diagnostic);
        Assert.Empty(diagnostic.Errors);
        Assert.Empty(diagnostic.Warnings);

        return document;
    }

    private HttpClient GetHttpClientForApplication(Type type, bool isMinimal)
    {
        if (isMinimal)
        {
            return SwaggerIntegrationTests.GetHttpClientForTestApplication(type);
        }
        else
        {
            var application = new TestSite(type, outputHelper);
            return application.BuildClient();
        }
    }
}

#endif
