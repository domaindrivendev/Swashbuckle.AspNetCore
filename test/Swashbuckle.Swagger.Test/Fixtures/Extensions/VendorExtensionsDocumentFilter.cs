using Swashbuckle.Swagger;

namespace Swashbuckle.Fixtures.Extensions
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.vendorExtensions.Add("X-property1", "value");
        }
    }
}