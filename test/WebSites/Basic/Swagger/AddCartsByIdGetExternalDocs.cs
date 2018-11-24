using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class AddCartsByIdGetExternalDocs : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.ExternalDocs = new OpenApiExternalDocs
            {
                Description = "External docs for CartsByIdGet",
                Url = new Uri("https://tempuri.org/carts-by-id-get")
            };
        }
    }
}
