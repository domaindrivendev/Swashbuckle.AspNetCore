using Swashbuckle.SwaggerGen;

namespace Swashbuckle.SwaggerGen.Fixtures.Extensions
{
    public class VendorExtensionsModelFilter : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.Extensions.Add("X-property1", "value");
        }
    }
}