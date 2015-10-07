using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-property1", "value");
        }
    }
}