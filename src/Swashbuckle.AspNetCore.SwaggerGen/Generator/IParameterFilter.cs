using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IParameterFilter
    {
        void Apply(OpenApiParameter parameter, ParameterFilterContext context);
    }

    public class ParameterFilterContext
    {
        public ParameterFilterContext(
            ApiParameterDescription apiParameterDescription,
            ISchemaRegistry schemaRegistry,
            ParameterInfo parameterInfo,
            PropertyInfo propertyInfo)
        {
            ApiParameterDescription = apiParameterDescription;
            SchemaRegistry = schemaRegistry;
            ParameterInfo = parameterInfo;
            PropertyInfo = propertyInfo;
        }

        public ApiParameterDescription ApiParameterDescription { get; }

        public ISchemaRegistry SchemaRegistry { get; }

        public ParameterInfo ParameterInfo { get; }

        public PropertyInfo PropertyInfo { get; }
    }
}
