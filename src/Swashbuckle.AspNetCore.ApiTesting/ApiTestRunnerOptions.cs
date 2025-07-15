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
    }

    public Dictionary<string, OpenApiDocument> OpenApiDocs { get; }

    public List<IContentValidator> ContentValidators { get; }

    public bool GenerateOpenApiFiles { get; set; }

    public string FileOutputRoot { get; set; }
}
