using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations.Test
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Extensions.Add("X-property1", "value");
        }
    }
}