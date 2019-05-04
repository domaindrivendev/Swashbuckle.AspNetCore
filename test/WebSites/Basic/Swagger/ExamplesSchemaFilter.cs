using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class ExamplesSchemaFilter : ISchemaFilter
    {
        public static IDictionary<string, OpenApiObject> Schemas = new Dictionary<string, OpenApiObject>
        {
            {
                "Product", new OpenApiObject
                {
                    [ "Id" ] = new OpenApiInteger(123),
                    [ "Description" ] = new OpenApiString("foobar"),
                    [ "Cost" ] = new OpenApiDouble(10.7)
                }
            }
        };

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            var typeName = context.Type.Name;
            schema.Example = !Schemas.ContainsKey(typeName) ? null : Schemas[typeName];
        }
    }
}
