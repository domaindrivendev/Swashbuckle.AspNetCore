using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal class JsonPrimitiveHandler : SchemaGeneratorHandler
    {
        public JsonPrimitiveHandler(SchemaGeneratorOptions schemaGeneratorOptions, SchemaGenerator schemaGenerator, JsonSerializerSettings jsonSerializerSettings)
            : base(schemaGeneratorOptions, schemaGenerator, jsonSerializerSettings)
        { }

        protected override bool CanGenerateSchemaFor(ModelMetadata modelMetadata, JsonContract jsonContract)
        {
            return jsonContract is JsonPrimitiveContract
                && (modelMetadata.UnderlyingOrModelType.IsEnum || FactoryMethodMap.ContainsKey(modelMetadata.UnderlyingOrModelType));
        }

        protected override OpenApiSchema GenerateSchemaFor(ModelMetadata modelMetadata, SchemaRepository schemaRepository, JsonContract jsonContract)
        {
            var jsonPrimitiveContract = (JsonPrimitiveContract)jsonContract;

            var underlyingType = modelMetadata.UnderlyingOrModelType;

            var schema = underlyingType.IsEnum
                ? GenerateEnumSchema(underlyingType, jsonPrimitiveContract)
                : FactoryMethodMap[underlyingType]();

            if (modelMetadata.IsReferenceOrNullableType)
                schema.Nullable = true;

            return schema;
        }

        private OpenApiSchema GenerateEnumSchema(Type enumType, JsonPrimitiveContract jsonPrimitiveContract)
        {
            var stringEnumConverter = (jsonPrimitiveContract.Converter as StringEnumConverter)
                ?? JsonSerializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            if (SchemaGeneratorOptions.DescribeAllEnumsAsStrings || (stringEnumConverter != null))
            {
                var describeInCamelCase = SchemaGeneratorOptions.DescribeStringEnumsInCamelCase
                    #if NETCOREAPP3_0
                    || (stringEnumConverter != null && stringEnumConverter.NamingStrategy is CamelCaseNamingStrategy);
                    #else
                    || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);
                    #endif

                var schema = FactoryMethodMap[typeof(string)]();

                schema.Enum = enumType.GetFields(BindingFlags.Public | BindingFlags.Static)
                    .Select(f =>
                        {
                            var enumMemberAttribute = f.GetCustomAttributes().OfType<EnumMemberAttribute>().FirstOrDefault();
                            var serialName = (enumMemberAttribute != null) ? enumMemberAttribute.Value : f.Name;
                            return (IOpenApiAny)(new OpenApiString(describeInCamelCase ? serialName.ToCamelCase() : serialName));
                        })
                    .Distinct()
                    .ToList();

                return schema;
            }
            else
            {
                var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                var schema = FactoryMethodMap[enumUnderlyingType]();

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

        private static readonly Dictionary<Type, Func<OpenApiSchema>> FactoryMethodMap = new Dictionary<Type, Func<OpenApiSchema>>
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
