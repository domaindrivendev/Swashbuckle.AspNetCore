using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface IParameterFilter
    {
        void Apply(IParameter parameter, ParameterFilterContext context);
    }

    public class ParameterFilterContext
    {
        public ParameterFilterContext(
            ApiParameterDescription apiParameterDescription,
            ControllerParameterDescriptor controllerParameterDescriptor,
            ISchemaRegistry schemaRegistry)
        {
            ApiParameterDescription = apiParameterDescription;
            ControllerParameterDescriptor = controllerParameterDescriptor;
            SchemaRegistry = schemaRegistry;
        }

        public ApiParameterDescription ApiParameterDescription { get; }

        public ControllerParameterDescriptor ControllerParameterDescriptor { get; }

        public ISchemaRegistry SchemaRegistry { get; }
    }
}
