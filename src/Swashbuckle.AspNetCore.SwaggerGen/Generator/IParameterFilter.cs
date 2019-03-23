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
            ISchemaGenerator schemaGenerator,
            SchemaRepository schemaRepository,
            ParameterInfo parameterInfo,
            PropertyInfo propertyInfo)
        {
            ApiParameterDescription = apiParameterDescription;
            SchemaGenerator = schemaGenerator;
            SchemaRepository = schemaRepository;
            ParameterInfo = parameterInfo;
            PropertyInfo = propertyInfo;
        }

        public ApiParameterDescription ApiParameterDescription { get; }

        public ISchemaGenerator SchemaGenerator { get; }

        public SchemaRepository SchemaRepository { get; }

        public ParameterInfo ParameterInfo { get; }

        public PropertyInfo PropertyInfo { get; }
    }
}
