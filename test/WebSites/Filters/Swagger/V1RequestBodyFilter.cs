using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Filters.Swagger
{
    /// <summary>
    /// This filter appends a message to request body descriptions for the v1 document.
    /// </summary>
    public class V1RequestBodyFilter : IRequestBodyFilter
    {
        public void Apply(OpenApiRequestBody requestBody, RequestBodyFilterContext context)
        {
            if (context.DocumentName != "v1")
            {
                return;
            }

            requestBody.Description += "<br/><br/><b>This request body will be removed, please update to the v2 API.</b>";
        }
    }
}
