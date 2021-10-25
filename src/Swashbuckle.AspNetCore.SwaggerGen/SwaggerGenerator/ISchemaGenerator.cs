using System;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateSchema(
            Type modelType,
            SchemaRepository schemaRepository,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null,
            ApiParameterRouteInfo routeInfo = null);
    }
}
