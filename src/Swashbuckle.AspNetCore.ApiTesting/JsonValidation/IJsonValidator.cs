using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public interface IJsonValidator
    {
        bool CanValidate(OpenApiSchema schema);

        bool Validate(
            OpenApiSchema schema,
            OpenApiDocument openApiDocument,
            JToken instance,
            out IEnumerable<string> errorMessages);
    }
}