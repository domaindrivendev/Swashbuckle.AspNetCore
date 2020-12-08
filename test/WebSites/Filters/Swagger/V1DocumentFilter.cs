using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Filters.Swagger
{
    /// <summary>
    /// This filter appends a message to the API description for the v1 document.
    /// </summary>
    public class V1DocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            if (context.DocumentName != "v1")
            {
                return;
            }

            swaggerDoc.Info.Description += "<br/><br/><b>This API will be removed, please update to the v2 API.</b>";
        }
    }
}
