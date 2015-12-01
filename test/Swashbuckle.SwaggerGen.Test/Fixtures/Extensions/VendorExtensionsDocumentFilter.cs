using Swashbuckle.SwaggerGen;

namespace Swashbuckle.SwaggerGen.Fixtures.Extensions
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-property1", "value");
        }
    }
}