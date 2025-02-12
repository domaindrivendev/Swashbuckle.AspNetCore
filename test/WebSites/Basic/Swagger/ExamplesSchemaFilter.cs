using System;
using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class ExamplesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            schema.Example = GetExampleOrNullFor(context.Type);
        }

        private static JsonObject GetExampleOrNullFor(Type type)
        {
            return type.Name switch
            {
                "Product" => new JsonObject
                {
                    ["id"] = 123,
                    ["description"] = "foobar",
                    ["price"] = 14.37d
                },
                _ => null,
            };
        }
    }
}
