using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class VendorExtensionsSchemaFilter : ISchemaFilter
    {
        public void Apply(Schema schema, SchemaFilterContext context)
        {
            schema.Extensions.Add("x-dotnet-type", context.SystemType.FullName);
        }
    }
}