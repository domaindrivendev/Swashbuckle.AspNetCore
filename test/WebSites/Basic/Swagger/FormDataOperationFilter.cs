using Microsoft.AspNetCore.Mvc;
using Swashbuckle.Swagger.Model;
using Swashbuckle.SwaggerGen.Generator;
using System.Linq;

namespace Basic.Swagger
{
    public class FormDataOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var formMediaType = context.ApiDescription.ActionAttributes()
                .OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .FirstOrDefault(mediaType => mediaType == "multipart/form-data");

            if (formMediaType != null)
                operation.Consumes = new[] { formMediaType };
        }
    }
}
