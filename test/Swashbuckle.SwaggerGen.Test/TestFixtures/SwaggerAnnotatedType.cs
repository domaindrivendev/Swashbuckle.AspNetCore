using Swashbuckle.SwaggerGen.Annotations;
using Swashbuckle.SwaggerGen.TestFixtures.Extensions;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    [SwaggerModelFilter(typeof(VendorExtensionsModelFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}