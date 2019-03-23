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
            var type = context.Type;
            schema.Example = GetExampleOrNullFor(type);
        }

        private IOpenApiAny GetExampleOrNullFor(Type systemType)
        {
            switch (systemType.Name)
            {
                case "Product":
                    return new OpenApiObject
                    {
                        [ "Id" ] = new OpenApiInteger(123),
                        [ "Description" ] = new OpenApiString("foobar")
                    };
                default:
                    return null;
            }
        }
    }
}
