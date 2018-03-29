using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.AzureFunctions.Filters
{
    public class RemoveBodyParameterFromGetFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (string.Equals(context.ApiDescription.HttpMethod, HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
            {
                var bodyParameters = operation.Parameters.Where(x => x.In == "body").ToList();
                foreach (var bodyParameter in bodyParameters)
                {
                    operation.Parameters.Remove(bodyParameter);
                }
            }
        }
    }
}