using System;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen.SchemaMappings;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Interface describing a service capable of deriving schemas for arbitrary C# types.
    /// </summary>
    public interface ISchemaGenerator
    {
        OpenApiSchema GenerateMemberSchema(MemberInfo memberInfo, SchemaRepository schemaRepository);
        OpenApiSchema GenerateParameterSchema(ParameterInfo parameterInfo, SchemaRepository schemaRepository);
        SchemaMapping GenerateTypeSchema(Type type);
    }
}
