using System;
using Xunit;
using Swashbuckle.SwaggerGen.Fixtures;

namespace Swashbuckle.SwaggerGen.Annotations
{
    public class ApplySwaggerModelFilterAttributesTests
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

        private ApplySwaggerModelFilterAttributes Subject()
        {
            return new ApplySwaggerModelFilterAttributes();
        }
    }
}