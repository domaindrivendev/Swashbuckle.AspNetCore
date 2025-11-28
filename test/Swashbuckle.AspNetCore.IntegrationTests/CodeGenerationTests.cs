#if NET10_0_OR_GREATER

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    [Theory]
    [InlineData(ClientGeneratorTool.Kiota, "https://petstore3.swagger.io/api/v3/openapi.json")]
    [InlineData(ClientGeneratorTool.NSwag, "https://petstore3.swagger.io/api/v3/openapi.json")]
    public async Task OpenApiDocument_Generates_Valid_Client_Code(ClientGeneratorTool tool, string openApiDocumentUrl)
    {
        // Arrange
        var generator = new ClientGenerator(outputHelper);

        using var project = await generator.GenerateFromUrlAsync(tool, openApiDocumentUrl);

        // Act and Assert
        await generator.CompileAsync(project.Path);
    }
}

#endif
