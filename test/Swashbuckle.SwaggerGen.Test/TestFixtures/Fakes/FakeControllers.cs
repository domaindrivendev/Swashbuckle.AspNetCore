using Swashbuckle.SwaggerGen.Annotations;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class FakeControllers
    {
        public class NotAnnotated
        {}

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public class AnnotatedWithSwaggerOperationFilter
        {}

        public class TestController
        {}
    }
}
