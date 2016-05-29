using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class VendorExtensionsDocumentFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Extensions.Add("X-property1", "value");
        }
    }
}