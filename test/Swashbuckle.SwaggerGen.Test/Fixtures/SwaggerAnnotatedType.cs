using Swashbuckle.SwaggerGen.Annotations;
using Swashbuckle.SwaggerGen.Fixtures.Extensions;

namespace Swashbuckle.SwaggerGen.Fixtures
{
    [SwaggerModelFilter(typeof(VendorExtensionsModelFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}