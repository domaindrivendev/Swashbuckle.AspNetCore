using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonPrimitiveHandler : SchemaGeneratorHandler
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public JsonPrimitiveHandler(
            SchemaGeneratorOptions schemaGeneratorOptions,
            ISchemaGenerator schemaGenerator,
            JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
        }

        protected override bool CanGenerateSchema(JsonContract jsonContract, out bool shouldBeReferenced)
        {
            if (jsonContract is JsonPrimitiveContract jsonPrimitiveContract)
            {
                shouldBeReferenced = jsonPrimitiveContract.UnderlyingType.IsEnum;
                return true;
            }

            shouldBeReferenced = false; return false;
        }

        protected override OpenApiSchema GenerateDefinitionSchema(JsonContract jsonContract, SchemaRepository schemaRepository)
        {
            var jsonPrimitiveContract = (JsonPrimitiveContract)jsonContract;

            return jsonPrimitiveContract.UnderlyingType.IsEnum
                ? GenerateEnumSchema(jsonPrimitiveContract)
                : PrimitiveTypeMap[jsonPrimitiveContract.UnderlyingType]();
        }

        private OpenApiSchema GenerateEnumSchema(JsonPrimitiveContract jsonPrimitiveContract)
        {
            var enumType = jsonPrimitiveContract.UnderlyingType;

            var stringEnumConverter = (jsonPrimitiveContract.Converter as StringEnumConverter)
                ?? _jsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (SchemaGeneratorOptions.DescribeAllEnumsAsStrings || stringEnumConverter != null)
            {
                var schema = PrimitiveTypeMap[typeof(string)]();

                var describeInCamelCase = SchemaGeneratorOptions.DescribeStringEnumsInCamelCase
#if NETCOREAPP3_0
                    || (stringEnumConverter != null && stringEnumConverter.NamingStrategy is CamelCaseNamingStrategy);
#else
                    || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);
#endif

                schema.Enum = enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f =>
                    {
                        var enumMemberAttribute = f.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault();
                        var enumValue = (enumMemberAttribute != null) ? enumMemberAttribute.Value : f.Name;
                        return (IOpenApiAny)(new OpenApiString(describeInCamelCase ? enumValue.ToCamelCase() : enumValue));
                    })
                    .Distinct()
                    .ToList();

                return schema;
            }
            else
            {
                var enumUnderlyingType = enumType.GetEnumUnderlyingType();

                var schema = PrimitiveTypeMap[enumUnderlyingType]();

                schema.Enum = enumType.GetEnumValues()
                    .Cast<object>()
                    .Select(value =>
                    {
                        value = Convert.ChangeType(value, enumUnderlyingType);
                        return OpenApiAnyFactory.TryCreateFor(schema, value, out IOpenApiAny openApiAny) ? openApiAny : null;

                    })
                    .Distinct()
                    .ToList();

                return schema;
            }
        }

        private static readonly Dictionary<Type, Func<OpenApiSchema>> PrimitiveTypeMap = new Dictionary<Type, Func<OpenApiSchema>>
        {
            { typeof(bool), () => new OpenApiSchema { Type = "boolean" } },
            { typeof(byte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(sbyte), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(short), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(ushort), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(int), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(uint), () => new OpenApiSchema { Type = "integer", Format = "int32" } },
            { typeof(long), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(ulong), () => new OpenApiSchema { Type = "integer", Format = "int64" } },
            { typeof(float), () => new OpenApiSchema { Type = "number", Format = "float" } },
            { typeof(double), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(decimal), () => new OpenApiSchema { Type = "number", Format = "double" } },
            { typeof(byte[]), () => new OpenApiSchema { Type = "string", Format = "byte" } },
            { typeof(string), () => new OpenApiSchema { Type = "string" } },
            { typeof(char), () => new OpenApiSchema { Type = "string" } },
            { typeof(DateTime), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(DateTimeOffset), () => new OpenApiSchema { Type = "string", Format = "date-time" } },
            { typeof(Guid), () => new OpenApiSchema { Type = "string", Format = "uuid" } },
            { typeof(Uri), () => new OpenApiSchema { Type = "string", Format = "uri" } },
            { typeof(TimeSpan), () => new OpenApiSchema { Type = "string", Format = "date-span" } },
        };
    }
}