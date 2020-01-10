using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Newtonsoft
{
    public class NewtonsoftEnumHandler : SchemaGeneratorHandler
    {
        private readonly SchemaGeneratorOptions _generatorOptions;
        private readonly IContractResolver _contractResolver;
        private readonly JsonSerializerSettings _serializerSettings;

        public NewtonsoftEnumHandler(
            SchemaGeneratorOptions generatorOptions,
            IContractResolver contractResolver,
            JsonSerializerSettings serializerSettings)
        {
            _generatorOptions = generatorOptions;
            _contractResolver = contractResolver;
            _serializerSettings = serializerSettings;
        }

        public override bool CanCreateSchemaFor(Type type, out bool shouldBeReferenced)
        {
            if (type.IsEnum)
            {
                shouldBeReferenced = !_generatorOptions.UseInlineDefinitionsForEnums;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        public override OpenApiSchema CreateSchema(Type type, SchemaRepository schemaRepository)
        {
            var jsonContract = _contractResolver.ResolveContract(type);

            var stringEnumConverter = (jsonContract.Converter as StringEnumConverter)
                ?? _serializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            // Temporary shim to support obsolete config options
            if (stringEnumConverter == null && _generatorOptions.DescribeAllEnumsAsStrings)
            {
                stringEnumConverter = new StringEnumConverter(_generatorOptions.DescribeStringEnumsInCamelCase);
            }

            var schema = (stringEnumConverter != null)
                ? EnumTypeMap[typeof(string)]()
                : EnumTypeMap[type.GetEnumUnderlyingType()]();

            if (stringEnumConverter != null)
            {
                schema.Enum = type.GetMembers(BindingFlags.Public | BindingFlags.Static)
                    .Select(member =>
                    {
                        var memberAttribute = member.GetCustomAttributes<EnumMemberAttribute>().FirstOrDefault();
                        var stringValue = GetConvertedEnumName(stringEnumConverter, (memberAttribute?.Value ?? member.Name), (memberAttribute?.Value != null));
                        return OpenApiAnyFactory.CreateFor(schema, stringValue);
                    })
                    .ToList();
            }
            else
            {
                schema.Enum = type.GetEnumValues()
                    .Cast<object>()
                    .Select(value => OpenApiAnyFactory.CreateFor(schema, value))
                    .ToList();
            }

            return schema;
        }

#if NETCOREAPP3_0
        private string GetConvertedEnumName(StringEnumConverter stringEnumConverter, string enumName, bool hasSpecifiedName)
        {
            if (stringEnumConverter.NamingStrategy != null)
                return stringEnumConverter.NamingStrategy.GetPropertyName(enumName, hasSpecifiedName);

            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
        }
#else
        private string GetConvertedEnumName(StringEnumConverter stringEnumConverter, string enumName, bool hasSpecifiedName)
        {
            return (stringEnumConverter.CamelCaseText)
                ? new CamelCaseNamingStrategy().GetPropertyName(enumName, hasSpecifiedName)
                : enumName;
        }
#endif

        private static readonly Dictionary<Type, Func<OpenApiSchema>> EnumTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            [ typeof(byte) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(sbyte) ] =  () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(short) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(ushort) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(int) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(uint) ] = () => new OpenApiSchema { Type = "integer", Format = "int32" },
            [ typeof(long) ] = () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(ulong) ] = () => new OpenApiSchema { Type = "integer", Format = "int64" },
            [ typeof(string) ] = () => new OpenApiSchema { Type = "string" }
        };
    }
}