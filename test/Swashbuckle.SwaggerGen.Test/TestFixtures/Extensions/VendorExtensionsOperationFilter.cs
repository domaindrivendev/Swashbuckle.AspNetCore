using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.TestFixtures.Extensions
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext contex)
        {
            operation.Extensions.Add("X-property1", "value");
        }
    }
}