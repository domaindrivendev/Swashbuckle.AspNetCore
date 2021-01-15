using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-foo", new OpenApiString("bar"));
            swaggerDoc.Extensions.Add("X-docName", new OpenApiString(context.DocumentName));
        }
    }
}