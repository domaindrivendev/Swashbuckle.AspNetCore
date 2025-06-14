using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models.Interfaces;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public interface ISchemaGenerator
{
    IOpenApiSchema GenerateSchema(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo = null,
        ParameterInfo parameterInfo = null,
        ApiParameterRouteInfo routeInfo = null);
}
