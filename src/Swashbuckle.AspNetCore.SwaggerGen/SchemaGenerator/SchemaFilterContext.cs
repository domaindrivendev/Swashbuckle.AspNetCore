using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class SchemaFilterContext
{
    public SchemaFilterContext(
        Type type,
        ISchemaGenerator schemaGenerator,
        SchemaRepository schemaRepository,
        MemberInfo memberInfo = null,
        ParameterInfo parameterInfo = null)
    {
        Type = type;
        SchemaGenerator = schemaGenerator;
        SchemaRepository = schemaRepository;
        MemberInfo = memberInfo;
        ParameterInfo = parameterInfo;
    }

    public Type Type { get; }

    public ISchemaGenerator SchemaGenerator { get; }

    public SchemaRepository SchemaRepository { get; }

    public MemberInfo MemberInfo { get; }

    public ParameterInfo ParameterInfo { get; }

    public string DocumentName => SchemaRepository.DocumentName;
}
