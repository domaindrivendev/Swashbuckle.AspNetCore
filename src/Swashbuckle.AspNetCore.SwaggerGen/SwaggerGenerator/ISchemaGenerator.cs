using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateSchema(
            Type modelType,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null);
    }
}
