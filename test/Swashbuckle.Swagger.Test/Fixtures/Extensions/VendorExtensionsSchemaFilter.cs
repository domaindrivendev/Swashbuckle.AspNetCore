using Swashbuckle.Swagger;

namespace Swashbuckle.Fixtures.Extensions
{
    public class VendorExtensionsSchemaFilter : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.vendorExtensions.Add("X-property1", "value");
        }
    }
}