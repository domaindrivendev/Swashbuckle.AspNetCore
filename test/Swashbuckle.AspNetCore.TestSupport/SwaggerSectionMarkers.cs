using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class SwaggerSectionMarkers : IOperationFilter, IDocumentFilter
    {
        public static void Enable(SwaggerGenOptions swaggerGenOptions)
        {
            swaggerGenOptions.SwaggerGeneratorOptions.OperationFilters.Add(new SwaggerSectionMarkers());
            swaggerGenOptions.SwaggerGeneratorOptions.DocumentFilters.Add(new SwaggerSectionMarkers());
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.OperationId = context.MethodInfo.Name;
            operation.Extensions.Add("x-end", new OpenApiString(context.MethodInfo.Name));

        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Components.Extensions.Add("x-end", new OpenApiString("components"));
        }
    }
}
