using System;
using Xunit;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class SwaggerAttributesSchemaFilterTests
    {
        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute()
        {
            IEnumerable<Schema> schemas;
            var filterContexts = new[]
            {
                FilterContextFor(typeof(SwaggerAnnotatedClass)),
                FilterContextFor(typeof(SwaggerAnnotatedStruct))
            };

            schemas = filterContexts.Select(c => {
                var schema = new Schema();
                Subject().Apply(schema, c);
                return schema;
            });

            Assert.All(schemas, s => Assert.NotEmpty(s.Extensions));
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            return new SchemaFilterContext(type, null, null);
        }

        private SwaggerAttributesSchemaFilter Subject()
        {
            return new SwaggerAttributesSchemaFilter(new Mock<IServiceProvider>().Object);
        }
    }
}