using Swashbuckle.SwaggerGen.Annotations;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    [SwaggerModelFilter(typeof(VendorExtensionsModelFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}