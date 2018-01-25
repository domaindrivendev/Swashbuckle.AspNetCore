using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
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

        [Produces("application/json")]
        public class AnnotatedWithProducesAttribute
        {}
    }
}