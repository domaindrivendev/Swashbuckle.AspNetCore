using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class ParameterFilterContext(
    ApiParameterDescription apiParameterDescription,
    ISchemaGenerator schemaGenerator,
    SchemaRepository schemaRepository,
    PropertyInfo propertyInfo = null,
    ParameterInfo parameterInfo = null)
{
    public ApiParameterDescription ApiParameterDescription { get; } = apiParameterDescription;

    public ISchemaGenerator SchemaGenerator { get; } = schemaGenerator;

    public SchemaRepository SchemaRepository { get; } = schemaRepository;

    public PropertyInfo PropertyInfo { get; } = propertyInfo;

    public ParameterInfo ParameterInfo { get; } = parameterInfo;

    public string DocumentName => SchemaRepository.DocumentName;
}
