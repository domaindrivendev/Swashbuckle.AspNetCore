using Swashbuckle.SwaggerGen;

namespace Swashbuckle.SwaggerGen.Fixtures.Extensions
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext contex)
        {
            operation.Extensions.Add("X-property1", "value");
        }
    }
}