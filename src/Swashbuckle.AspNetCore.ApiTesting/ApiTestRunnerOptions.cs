using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.ApiTesting;

public class ApiTestRunnerOptions
{
    public ApiTestRunnerOptions()
    {
        OpenApiDocs = [];
        ContentValidators = [new JsonContentValidator()];
        GenerateOpenApiFiles = false;
        FileOutputRoot = null;
        OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
    }

    public Dictionary<string, OpenApiDocument> OpenApiDocs { get; }

    public List<IContentValidator> ContentValidators { get; }

    public bool GenerateOpenApiFiles { get; set; }

    public string FileOutputRoot { get; set; }

    /// <summary>
    /// Gets or sets the version of the OpenAPI specification to use when writing OpenAPI documents
    /// if <see cref="GenerateOpenApiFiles"/> is <see langword="true"/>.
    /// </summary>
    /// <remarks>
    /// The default value is <see cref="OpenApiSpecVersion.OpenApi3_0"/>.
    /// </remarks>
    public OpenApiSpecVersion OpenApiVersion { get; set; }
}
