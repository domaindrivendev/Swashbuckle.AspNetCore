using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class OperationFilterContext(
    ApiDescription apiDescription,
    ISchemaGenerator schemaRegistry,
    SchemaRepository schemaRepository,
    MethodInfo methodInfo)
{
    public ApiDescription ApiDescription { get; } = apiDescription;

    public ISchemaGenerator SchemaGenerator { get; } = schemaRegistry;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public MethodInfo MethodInfo { get; } = methodInfo;

    public string DocumentName => SchemaRepository.DocumentName;
}
