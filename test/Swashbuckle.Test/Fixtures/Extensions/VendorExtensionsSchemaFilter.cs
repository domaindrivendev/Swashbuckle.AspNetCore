using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.Test.Fixtures.Extensions
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaGenerator schemaGenerator, Type type)
        {
            schema.vendorExtensions.Add("X-property1", "value");
        }
    }
}