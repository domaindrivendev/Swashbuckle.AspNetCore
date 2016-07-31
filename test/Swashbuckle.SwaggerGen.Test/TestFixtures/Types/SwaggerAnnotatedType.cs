using Swashbuckle.SwaggerGen.Annotations;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}