using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Filters.Swagger
{
    /// <summary>
    /// This filter appends a message to schema descriptions for the v1 document.
    /// </summary>
    public class V1SchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.SchemaRepository.DocumentName != "v1")
            {
                return;
            }

            schema.Description += "<br/><br/><b>This schema will be removed, please update to the v2 API.</b>";
        }
    }
}
