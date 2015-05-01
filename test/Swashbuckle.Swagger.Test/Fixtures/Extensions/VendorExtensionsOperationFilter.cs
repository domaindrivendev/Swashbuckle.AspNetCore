using Swashbuckle.Swagger;

namespace Swashbuckle.Fixtures.Extensions
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext contex)
        {
            operation.Extensions.Add("X-property1", "value");
        }
    }
}