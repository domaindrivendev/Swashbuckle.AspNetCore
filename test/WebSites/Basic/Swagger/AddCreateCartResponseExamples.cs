using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using Basic.Controllers;

namespace Basic.Swagger
{
    public class AddCreateCartResponseExamples : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            operation.Responses["200"].Examples = new Dictionary<string, Cart>
            {
                { "application/json", new Cart { Id = 333 } }
            };
        }
    }
}
