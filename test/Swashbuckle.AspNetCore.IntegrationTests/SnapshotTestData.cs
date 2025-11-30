using System.Reflection;
using System.Text.Json;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public static class SnapshotTestData
{
    private static string _projectRoot =
        typeof(SnapshotTestData).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First((p) => p.Key is "ProjectRoot")
            .Value!;

    public static string SnapshotsPath() => Path.Combine(_projectRoot, "snapshots");

    public static string SchemasPath() => Path.Combine(_projectRoot, "schemas");

    public static TheoryData<string, Version> Snapshots()
    {
        var testCases = new TheoryData<string, Version>();
        var snapshotsPath = Path.Combine(_projectRoot, "snapshots");

        foreach (var path in Directory.EnumerateFiles(snapshotsPath, "*.txt", SearchOption.AllDirectories))
        {
            using var snapshot = File.OpenRead(path);
            using var document = JsonDocument.Parse(snapshot);

            if (!document.RootElement.TryGetProperty("openapi", out var property) &&
                !document.RootElement.TryGetProperty("swagger", out property))
            {
                continue;
            }

            if (!Version.TryParse(property.GetString(), out var version))
            {
                continue;
            }

            var relativePath = Path.GetRelativePath(snapshotsPath, path);

            testCases.Add(relativePath, version);
        }

        return testCases;
    }
}
