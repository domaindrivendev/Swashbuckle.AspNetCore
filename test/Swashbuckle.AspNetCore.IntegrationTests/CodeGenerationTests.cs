#if NET10_0_OR_GREATER

using System.Reflection;
using System.Text.Json;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
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
}

#endif
