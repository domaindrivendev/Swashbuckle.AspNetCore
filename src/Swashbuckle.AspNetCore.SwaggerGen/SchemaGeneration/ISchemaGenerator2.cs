using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public interface ISchemaGenerator2
    {
        OpenApiSchema GenerateForType(Type type, SchemaRepository schemaRepository);

        OpenApiSchema GenerateForMember(MemberInfo memberInfo, SchemaRepository schemaRepository);

        OpenApiSchema GenerateForParameter(ParameterInfo parameterInfo, SchemaRepository schemaRepository);
    }
}
