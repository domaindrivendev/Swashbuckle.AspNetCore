using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

// SchemaGenerator.GenerateSchema isn't virtual, so a derived generator customizes it by re-implementing the interface
public class ReimplementingSchemaGenerator(
    SchemaGeneratorOptions generatorOptions,
    ISerializerDataContractResolver serializerDataContractResolver)
    : SchemaGenerator(generatorOptions, serializerDataContractResolver), ISchemaGenerator
{
    public List<(Type Type, IOpenApiSchema Schema)> GeneratedSchemas { get; } = [];

    IOpenApiSchema ISchemaGenerator.GenerateSchema(
        Type modelType,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo,
        ParameterInfo parameterInfo,
        ApiParameterRouteInfo routeInfo)
    {
        var schema = GenerateSchema(modelType, schemaRepository, memberInfo, parameterInfo, routeInfo);
        GeneratedSchemas.Add((modelType, schema));
        return schema;
    }
}
