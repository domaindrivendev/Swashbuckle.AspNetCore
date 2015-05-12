using Swashbuckle.Swagger;

namespace Swashbuckle.Fixtures.Extensions
{
    public class VendorExtensionsModelFilter : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.Extensions.Add("X-property1", "value");
        }
    }
}