using System;
using Microsoft.OpenApi.Any;
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

        private static IOpenApiAny GetExampleOrNullFor(Type type)
        {
            return type.Name switch
            {
                "Product" => new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(123),
                    ["description"] = new OpenApiString("foobar"),
                    ["price"] = new OpenApiDouble(14.37)
                },
                _ => null,
            };
        }
    }
}
