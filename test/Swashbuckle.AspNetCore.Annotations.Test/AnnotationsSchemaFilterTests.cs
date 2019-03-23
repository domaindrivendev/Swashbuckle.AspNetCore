using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using Xunit;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class AnnotationsSchemaFilterTests
    {
        [Fact]
        public void Apply_DelegatesToSpecifiedFilter_IfTypeDecoratedWithFilterAttribute()
        {
            IEnumerable<OpenApiSchema> schemas;
            var filterContexts = new[]
            {
                FilterContextFor(typeof(SwaggerAnnotatedClass)),
                FilterContextFor(typeof(SwaggerAnnotatedStruct))
            };

            schemas = filterContexts.Select(c =>
            {
                var schema = new OpenApiSchema();
                Subject().Apply(schema, c);
                return schema;
            });

            Assert.All(schemas, s => Assert.NotEmpty(s.Extensions));
        }

        private SchemaFilterContext FilterContextFor(Type type)
        {
            return new SchemaFilterContext(type, null, null, null);
        }

        private AnnotationsSchemaFilter Subject()
        {
            return new AnnotationsSchemaFilter(null);
        }
    }
}