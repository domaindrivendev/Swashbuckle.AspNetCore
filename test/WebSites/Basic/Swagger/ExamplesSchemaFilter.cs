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

        private IOpenApiAny GetExampleOrNullFor(Type type)
        {
            switch (type.Name)
            {
                case "Product":
                    return new OpenApiObject
                    {
                        [ "id" ] = new OpenApiInteger(123),
                        [ "description" ] = new OpenApiString("foobar"),
                        [ "price" ] = new OpenApiDouble(14.37)
                    };
                default:
                    return null;
            }
        }
    }
}
