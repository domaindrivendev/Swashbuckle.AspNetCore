using Swashbuckle.Swagger.Annotations;
using Swashbuckle.Swagger.Fixtures.Extensions;

namespace Swashbuckle.Swagger.Fixtures.ApiDescriptions
{
    public class ControllerFixtures
    {
        public class NotAnnotated
        {}

        [SwaggerResponseRemoveDefaults]
        public class AnnotatedWithSwaggerResponseRemoveDefaults
        {}

        [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
        public class AnnotatedWithSwaggerOperationFilter
        {}

        [SwaggerResponse(200, "Controller defined 200", typeof(ComplexType))]
        [SwaggerResponse(400, "Controller defined 400", typeof(ComplexType))]
        public class AnnotatedWithSwaggerResponses
        {}

        public class TestController
        {}
    }
}
