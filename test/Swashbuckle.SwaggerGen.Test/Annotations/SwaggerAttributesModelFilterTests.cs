using System;
using Xunit;
using Moq;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.TestFixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class SwaggerAttributesModelFilterTests
    {
        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute()
        {
            var schema = new Schema { };
            var filterContext = FilterContextFor(typeof(SwaggerAnnotatedType));

            Subject().Apply(schema, filterContext);

            Assert.NotEmpty(schema.Extensions);
        }

        private ModelFilterContext FilterContextFor(Type type)
        {
            return new ModelFilterContext(type, null, null);
        }

        private SwaggerAttributesModelFilter Subject()
        {
            return new SwaggerAttributesModelFilter(new Mock<IServiceProvider>().Object);
        }
    }
}