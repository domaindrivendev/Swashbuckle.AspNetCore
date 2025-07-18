using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class OperationFilterContext(
    ApiDescription apiDescription,
    ISchemaGenerator schemaRegistry,
    SchemaRepository schemaRepository,
    OpenApiDocument document,
    MethodInfo methodInfo)
{
    public ApiDescription ApiDescription { get; } = apiDescription;

    public ISchemaGenerator SchemaGenerator { get; } = schemaRegistry;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public MethodInfo MethodInfo { get; } = methodInfo;

    public OpenApiDocument Document { get; } = document;

    public string DocumentName => SchemaRepository.DocumentName;
}
