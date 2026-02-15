#if NET10_0_OR_GREATER

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
        
        foreach (var testCase in SnapshotTestData.Snapshots())
        {
            var path = testCase.Data.Item1;
            var documentVersion = testCase.Data.Item2;

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
    public async Task GeneratesValidClient(ClientGeneratorTool tool, string snapshot)
    {
        // Arrange
        var generator = new ClientGenerator(outputHelper);
        var snapshotPath = Path.Combine(SnapshotTestData.SnapshotsPath(), snapshot);

        var document = await File.ReadAllTextAsync(snapshotPath, TestContext.Current.CancellationToken);

        using var project = await generator.GenerateFromStringAsync(tool, document);

        // Act and Assert
        await generator.CompileAsync(project.Path);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{tool}:{snapshot}"));
        var hashString = Convert.ToHexString(hash).ToLowerInvariant()[..16];

        outputHelper.WriteLine($"{nameof(tool)}={tool}, {nameof(snapshot)}={snapshot} [{hashString}]");

        await VerifyDirectory(
            project.Path,
            pattern: "*.cs",
            include: (p) => !p.Contains("bin") && !p.Contains("obj"),
            options: new() { RecurseSubdirectories = true })
            .UseDirectory("snapshots/code")
            .UseFileName($"{nameof(GeneratesValidClient)}_{hashString}")
            .AddScrubber((builder) =>
            {
                var content = builder.ToString();
                int start = content.IndexOf("RequestAdapter.BaseUrl = \"file://");
                if (start >= 0)
                {
                    int end = content.IndexOf(';', start);
                    builder.Replace(
                        content[start..(end + 1)],
                        "RequestAdapter.BaseUrl = \"file://{TempPath}\";");
                }
            });
    }

    [Fact]
    public async Task VerifyKiotaTodoAppClient()
    {
        await VerifyDirectory(
            Path.Combine(GetProjectRoot(), "KiotaTodoClient"),
            pattern: "*.cs",
            options: new() { RecurseSubdirectories = true }).UseDirectory("snapshots");
    }

    [Fact]
    public async Task VerifyNSwagTodoAppClient()
    {
        await VerifyDirectory(
            Path.Combine(GetProjectRoot(), "NSwagTodoClient"),
            pattern: "*.cs",
            options: new() { RecurseSubdirectories = true }).UseDirectory("snapshots");
    }

    private static string GetProjectRoot() =>
        typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "ProjectRoot")
            .Value!;
}

#endif
