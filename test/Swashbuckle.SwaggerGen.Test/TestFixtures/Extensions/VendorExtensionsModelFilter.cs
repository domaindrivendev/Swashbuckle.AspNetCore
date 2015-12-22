using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.TestFixtures.Extensions
{
    public class VendorExtensionsModelFilter : IModelFilter
    {
        public void Apply(Schema model, ModelFilterContext context)
        {
            model.Extensions.Add("X-property1", "value");
        }
    }
}