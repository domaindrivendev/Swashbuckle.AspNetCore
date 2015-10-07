using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger.Fixtures.Extensions;

namespace Swashbuckle.Swagger.Fixtures
{
    [SwaggerModelFilter(typeof(VendorExtensionsModelFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}