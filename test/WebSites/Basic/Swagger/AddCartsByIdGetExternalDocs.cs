using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Basic.Swagger
{
    public class AddCartsByIdGetExternalDocs : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.ExternalDocs = new ExternalDocs
            {
                Description = "External docs for CartsByIdGet",
                Url = "https://tempuri.org/carts-by-id-get"
            };
        }
    }
}
