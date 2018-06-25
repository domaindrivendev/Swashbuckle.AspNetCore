using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
