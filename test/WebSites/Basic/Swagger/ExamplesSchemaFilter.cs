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
            schema.Example = GetExampleOrNullFor(context.JsonContract.UnderlyingType);
        }

        private IOpenApiAny GetExampleOrNullFor(Type type)
        {
            switch (type.Name)
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
