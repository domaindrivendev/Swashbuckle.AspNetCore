using System;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaGenerator schemaGenerator, Type type)
        {
            schema.vendorExtensions.Add("X-property1", "value");
        }
    }
}