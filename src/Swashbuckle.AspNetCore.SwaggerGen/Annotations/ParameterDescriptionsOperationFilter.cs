using System.ComponentModel;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class ParameterDescriptionsOperationFilter : IOperationFilter
    {
        public void Apply(Operation operation, OperationFilterContext context)
        {
            var parameterDescriptions = context.ApiDescription.ParameterDescriptions;
            for (var index = 0; index < parameterDescriptions.Count; index++)
            {
                var modelMetaData = parameterDescriptions[index].ModelMetadata as DefaultModelMetadata;

                var descriptionAttribute =
                    modelMetaData?
                        .Attributes?
                        .Attributes?
                        .LastOrDefault(x => x.GetType() == typeof(DescriptionAttribute))
                        as DescriptionAttribute;

                operation.Parameters[index].Description = descriptionAttribute?.Description;
            }
        }
    }
}
