using System;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Fixtures.Extensions
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, ISchemaRegistry schemaRegistry, Type type)
        {
            schema.vendorExtensions.Add("X-property1", "value");
        }
    }
}