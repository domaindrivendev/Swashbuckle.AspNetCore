using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class ParameterFilterContext
{
    public ParameterFilterContext(
        ApiParameterDescription apiParameterDescription,
        ISchemaGenerator schemaGenerator,
        SchemaRepository schemaRepository,
        PropertyInfo propertyInfo = null,
        ParameterInfo parameterInfo = null)
    {
        ApiParameterDescription = apiParameterDescription;
        SchemaGenerator = schemaGenerator;
        SchemaRepository = schemaRepository;
        PropertyInfo = propertyInfo;
        ParameterInfo = parameterInfo;
    }

    public ApiParameterDescription ApiParameterDescription { get; }

    public ISchemaGenerator SchemaGenerator { get; }

    public SchemaRepository SchemaRepository { get; }

    public PropertyInfo PropertyInfo { get; }

    public ParameterInfo ParameterInfo { get; }

    public string DocumentName => SchemaRepository.DocumentName;
}
