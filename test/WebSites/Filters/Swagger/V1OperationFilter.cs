using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Filters.Swagger
{
    /// <summary>
    /// This filter appends a message to operation descriptions for the v1 document.
    /// </summary>
    public class V1OperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context.DocumentName != "v1")
            {
                return;
            }

            operation.Description += "<br/><br/><b>This operation will be removed, please update to the v2 API.</b>";
        }
    }
}
