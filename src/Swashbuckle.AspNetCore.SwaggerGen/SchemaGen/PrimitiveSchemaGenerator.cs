using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class PrimitiveSchemaGenerator : ChainableSchemaGenerator
    {
        public PrimitiveSchemaGenerator(
            SchemaGeneratorOptions options,
            ISchemaGenerator rootGenerator,
            IContractResolver contractResolver,
            JsonSerializerSettings serializerSettings) : base(options, rootGenerator, contractResolver)
        {
            _serializerSettings = serializerSettings;
        }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return ContractResolver.ResolveContract(type) is JsonPrimitiveContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonPrimitiveContract = (JsonPrimitiveContract)ContractResolver.ResolveContract(type);

            var schema = jsonPrimitiveContract.UnderlyingType.IsEnum
                ? GenerateEnumSchema(jsonPrimitiveContract)
                : FactoryMethodMap[jsonPrimitiveContract.UnderlyingType]();

            return schema;
        }

        private OpenApiSchema GenerateEnumSchema(JsonPrimitiveContract jsonPrimitiveContract)
        {
            var stringEnumConverter = _serializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault()
                ?? (jsonPrimitiveContract.Converter as StringEnumConverter); 

            var describeAsString = Options.DescribeAllEnumsAsStrings
                || (stringEnumConverter != null);

            var describeInCamelCase = Options.DescribeStringEnumsInCamelCase
                || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);

            var enumType = jsonPrimitiveContract.UnderlyingType;
            var enumUnderlyingType = describeAsString ? typeof(string) : enumType.GetEnumUnderlyingType();

            var schema = FactoryMethodMap[enumUnderlyingType]();

            if (describeAsString)
            {
                schema.Enum = enumType.GetEnumNames()
                    .Select(name =>
                    {
                        name = describeInCamelCase ? name.ToCamelCase() : name;
                        return OpenApiAnyFactory.TryCreateFrom(name, out IOpenApiAny openApiAny) ? openApiAny : null;
                    })
                    .ToList();
            }
            else
            {
                schema.Enum = enumType.GetEnumValues()
                    .Cast<object>()
                    .Select(value =>
                    {
                        value = Convert.ChangeType(value, enumUnderlyingType);
                        return OpenApiAnyFactory.TryCreateFrom(value, out IOpenApiAny openApiAny) ? openApiAny : null;
                    })
                    .ToList();
            }

            return schema;
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
            { typeof(Uri), () => new OpenApiSchema { Type = "string" } }
        };
        private readonly JsonSerializerSettings _serializerSettings;
    }
}
