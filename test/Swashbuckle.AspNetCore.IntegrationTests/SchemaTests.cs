using NJsonSchema;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class SchemaTests
{
    [Theory]
    [MemberData(nameof(SnapshotTestData.Snapshots), MemberType = typeof(SnapshotTestData))]
    public async Task OpenApiDocumentsAreValid(string snapshot, Version version)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var schemaPath = Path.Combine(SnapshotTestData.SchemasPath(), $"schema.{version.Major}.{version.Minor}.json");
        var snapshotPath = Path.Combine(SnapshotTestData.SnapshotsPath(), snapshot);

        var schema = await JsonSchema.FromFileAsync(schemaPath, cancellationToken);
        var specification = await File.ReadAllTextAsync(snapshotPath, cancellationToken);

        // Act
        var actual = schema.Validate(specification);

        // Assert
        Assert.NotNull(actual);
        Assert.Empty(actual);
    }
}
