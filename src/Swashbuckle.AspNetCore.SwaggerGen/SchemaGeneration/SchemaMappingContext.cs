using System;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SchemaMappingContext
    {
        public SchemaMappingContext(
            Type type,
            SchemaRepository schemaRepository,
            ISchemaGenerator schemaGenerator,
            MemberInfo memberInfo = null,
            ParameterInfo parameterInfo = null)
        {
            Type = type;
            SchemaRepository = schemaRepository;
            SchemaGenerator = schemaGenerator;
            MemberInfo = memberInfo;
            ParameterInfo = parameterInfo;
        }

        public Type Type { get; }
        public SchemaRepository SchemaRepository { get; }
        public ISchemaGenerator SchemaGenerator { get; }
        public MemberInfo MemberInfo { get; }
        public ParameterInfo ParameterInfo { get; }
    }
}