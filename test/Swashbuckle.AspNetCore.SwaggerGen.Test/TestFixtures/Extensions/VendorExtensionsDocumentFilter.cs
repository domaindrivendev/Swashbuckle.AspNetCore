using System;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            context.SchemaGenerator.GenerateSchemaFor(typeof(DateTime), context.SchemaRepository);
            swaggerDoc.Extensions.Add("X-property1", new OpenApiString("value"));
        }
    }
}