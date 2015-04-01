using System;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext contex)
        {
            operation.vendorExtensions.Add("X-property1", "value");
        }
    }
}