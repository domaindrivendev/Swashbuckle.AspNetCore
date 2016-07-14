using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using System;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            context.SchemaRegistry.GetOrRegister(typeof(DateTime));
            swaggerDoc.Extensions.Add("X-property1", "value");
        }
    }
}