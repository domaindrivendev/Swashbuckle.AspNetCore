using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class RecordingSchemaGenerator(ISchemaGenerator schemaGenerator) : ISchemaGenerator
{
    public List<(Type Type, IOpenApiSchema Schema)> GeneratedSchemas { get; } = [];

    public IOpenApiSchema GenerateSchema(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo = null,
        ParameterInfo parameterInfo = null,
        ApiParameterRouteInfo routeInfo = null)
    {
        var schema = schemaGenerator.GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
        GeneratedSchemas.Add((modelType, schema));
        return schema;
    }
}
