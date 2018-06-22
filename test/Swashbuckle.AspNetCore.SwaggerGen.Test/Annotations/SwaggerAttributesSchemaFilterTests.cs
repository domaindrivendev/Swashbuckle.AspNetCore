using System;
using Xunit;
using Moq;
using Swashbuckle.AspNetCore.Swagger;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
            var metaData = new EmptyModelMetadataProvider().GetMetadataForType(type);
            return new SchemaFilterContext(type, metaData, null, null);
        }

        private SwaggerAttributesSchemaFilter Subject()
        {
            return new SwaggerAttributesSchemaFilter(new Mock<IServiceProvider>().Object);
        }
    }
}