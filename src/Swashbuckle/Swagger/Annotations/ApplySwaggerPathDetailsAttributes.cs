using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.Swagger;
using Swashbuckle.Swagger.Annotations;

namespace Swashbuckle.Swagger.Annotations {
    public class ApplySwaggerPathDetailsAttributes : IOperationFilter {
        public void Apply(Operation operation, OperationFilterContext context) {
            var attr = context.ApiDescription.GetActionAttributes()
                .OfType<SwaggerPathDetailsAttribute>()
                .FirstOrDefault();

            if (attr != null) {
                operation.Summary = attr.Summary;
                operation.Description = attr.ImplementationNotes;
            }
        }
    }
}
