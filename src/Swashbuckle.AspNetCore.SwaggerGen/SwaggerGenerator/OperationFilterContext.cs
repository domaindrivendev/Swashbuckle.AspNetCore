using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class OperationFilterContext
{
    public OperationFilterContext(
        ApiDescription apiDescription,
        ISchemaGenerator schemaRegistry,
        SchemaRepository schemaRepository,
        MethodInfo methodInfo)
    {
        ApiDescription = apiDescription;
        SchemaGenerator = schemaRegistry;
        SchemaRepository = schemaRepository;
        MethodInfo = methodInfo;
    }

    public ApiDescription ApiDescription { get; }

    public ISchemaGenerator SchemaGenerator { get; }

    public SchemaRepository SchemaRepository { get; }

    public MethodInfo MethodInfo { get; }

    public string DocumentName => SchemaRepository.DocumentName;
}
