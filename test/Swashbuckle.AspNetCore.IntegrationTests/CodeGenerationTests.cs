#if NET10_0_OR_GREATER

using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public partial class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    private const string SnapshotsDirectory = "snapshots/code";

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
                var fileName = Path.GetFileNameWithoutExtension(path);

                if (fileName.Contains("Basic.Startup"))
                {
                    if (tool is ClientGeneratorTool.NSwag)
                    {
                        // NSwag doesn't generate valid compilation due to a missing FileResponse type
                        continue;
                    }

                    if (!fileName.Contains("=v1."))
                    {
                        // Ignore duplicative test cases for Swagger 2.0 and OpenAPI 3.1
                        continue;
                    }
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

        var projectPath = project.Path;

        await VerifyDirectory(
            projectPath,
            pattern: "*.cs",
            include: (p) => !IsBuildOutput(projectPath, p),
            options: new() { RecurseSubdirectories = true })
            .UseDirectory(SnapshotsDirectory)
            .UseFileName($"{nameof(GeneratesValidClient)}_{hashString}")
            .ScrubLinesWithReplace(line => GeneratedCodeRegex().Replace(line, "[GeneratedCode]"))
            .ScrubLinesWithReplace(line => NSwagGenerationComment().Replace(line, "//     Generated using the NSwag toolchain"))
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
            options: new() { RecurseSubdirectories = true }).UseDirectory(SnapshotsDirectory);
    }

    [Fact]
    public async Task VerifyNSwagTodoAppClient()
    {
        await VerifyDirectory(
            Path.Combine(GetProjectRoot(), "NSwagTodoClient"),
            pattern: "*.cs",
            options: new() { RecurseSubdirectories = true }).UseDirectory(SnapshotsDirectory);
    }

    [Theory]
    [InlineData("Client.cs", false)]
    [InlineData("Models/Product.cs", false)]
    [InlineData("bin/Release/net10.0/Client.cs", true)]
    [InlineData("obj/Release/net10.0/Client.AssemblyInfo.cs", true)]
    [InlineData("obj/Client.GlobalUsings.g.cs", true)]
    [InlineData("Binary/Objects/Client.cs", false)]
    public static void IsBuildOutput_Only_Matches_Directory_Segments_Under_Project(string relativePath, bool expected)
    {
        // Arrange - Temporary directory names are random and can contain "bin" or "obj" as substrings
        var root = Path.Combine(Path.GetTempPath(), "sfkobjtq.3yc");
        var path = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

        // Act
        var actual = IsBuildOutput(root, path);

        // Assert
        Assert.Equal(expected, actual);
    }

    private static bool IsBuildOutput(string projectPath, string filePath)
    {
        var relativePath = Path.GetRelativePath(projectPath, filePath);
        var segments = relativePath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);

        // Only exclude the project's own build output directories, not paths that
        // happen to contain "bin" or "obj" (e.g. a random temporary directory name)
        return segments.Length > 1 && segments[..^1].Any((p) => p is "bin" or "obj");
    }

    private static string GetProjectRoot() =>
        typeof(CodeGenerationTests).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "ProjectRoot")
            .Value!;

    [GeneratedRegex(@"\[System\.CodeDom\.Compiler\.GeneratedCode\(\"".*\""\)]")]
    private static partial Regex GeneratedCodeRegex();

    [GeneratedRegex(@"^//     Generated using the NSwag toolchain.*$")]
    private static partial Regex NSwagGenerationComment();
}

#endif
