using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsModelFilter : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.Extensions.Add("X-property1", "value");
        }
    }
}