using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Fixtures.Extensions;

namespace Swashbuckle.Fixtures
{
    [SwaggerModelFilter(typeof(VendorExtensionsModelFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}