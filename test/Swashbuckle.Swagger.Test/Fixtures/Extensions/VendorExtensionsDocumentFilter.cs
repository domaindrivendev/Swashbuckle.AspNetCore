using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.vendorExtensions.Add("X-property1", "value");
        }
    }
}