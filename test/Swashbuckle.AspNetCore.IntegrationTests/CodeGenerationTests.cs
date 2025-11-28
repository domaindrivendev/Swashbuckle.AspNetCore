#if NET10_0_OR_GREATER

using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.IntegrationTests;

/// <summary>
/// Tests that validate that OpenAPI documents produce valid C# code when used with code generation tools.
/// </summary>
public class CodeGenerationTests(ITestOutputHelper outputHelper)
{
    public static TheoryData<ClientGeneratorTool, string> TestCases()
    {
        var testCases = new TheoryData<ClientGeneratorTool, string>();

        (string Url, string Format, OpenApiSpecVersion Version)[] urls =
        [
            ("https://petstore.swagger.io/v2/swagger.json", "json", OpenApiSpecVersion.OpenApi2_0),
            ("https://petstore.swagger.io/v2/swagger.yaml", "yaml", OpenApiSpecVersion.OpenApi2_0),
            ("https://petstore3.swagger.io/api/v3/openapi.json", "json", OpenApiSpecVersion.OpenApi3_0),
            ("https://petstore3.swagger.io/api/v3/openapi.yaml", "yaml", OpenApiSpecVersion.OpenApi3_0),
            ////("https://petstore31.swagger.io/api/v31/openapi.json", "json", OpenApiSpecVersion.OpenApi3_1),
            ////("https://petstore31.swagger.io/api/v31/openapi.yaml", "yaml", OpenApiSpecVersion.OpenApi3_1),
        ];

        foreach (var tool in Enum.GetValues<ClientGeneratorTool>())
        {
            foreach ((var url, var format, var version) in urls)
            {
                if (ClientGenerator.IsSupported(tool, format, version))
                {
                    testCases.Add(tool, url);
                }
            }
        }

        return testCases;
    }

    [Theory]
    [MemberData(nameof(TestCases))]
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
