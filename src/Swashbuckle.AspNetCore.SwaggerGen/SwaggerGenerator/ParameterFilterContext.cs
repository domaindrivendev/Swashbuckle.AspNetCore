using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class ParameterFilterContext(
    ApiParameterDescription apiParameterDescription,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository,
    OpenApiDocument document,
    PropertyInfo propertyInfo = null,
    ParameterInfo parameterInfo = null)
{
    public ApiParameterDescription ApiParameterDescription { get; } = apiParameterDescription;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public PropertyInfo PropertyInfo { get; } = propertyInfo;

    public ParameterInfo ParameterInfo { get; } = parameterInfo;

    public OpenApiDocument Document { get; } = document;

    public string DocumentName => SchemaRepository.DocumentName;
}
