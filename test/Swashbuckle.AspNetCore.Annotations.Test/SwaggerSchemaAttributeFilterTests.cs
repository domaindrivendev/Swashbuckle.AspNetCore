using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class SwaggerSchemaAttributeFilterTests
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

            schemas = filterContexts.Select(c =>
            {
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

        private SwaggerSchemaAttributeFilter Subject()
        {
            return new SwaggerSchemaAttributeFilter(null);
        }
    }
}