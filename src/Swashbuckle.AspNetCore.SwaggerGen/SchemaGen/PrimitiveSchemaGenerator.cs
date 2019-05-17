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
        private readonly JsonSerializerSettings _serializerSettings;

        public PrimitiveSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            JsonSerializerSettings serializerSettings,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
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

            if (jsonPrimitiveContract.UnderlyingType.IsEnum)
                return GenerateEnumSchema(jsonPrimitiveContract);

            if (FactoryMethodMap.ContainsKey(jsonPrimitiveContract.UnderlyingType))
                return FactoryMethodMap[jsonPrimitiveContract.UnderlyingType]();

            return new OpenApiSchema { Type = "string" };
        }

        private OpenApiSchema GenerateEnumSchema(JsonPrimitiveContract jsonPrimitiveContract)
        {
            var stringEnumConverter = (jsonPrimitiveContract.Converter as StringEnumConverter)
                ?? _serializerSettings.Converters.OfType<StringEnumConverter>().FirstOrDefault();

            var describeAsString = Options.DescribeAllEnumsAsStrings
                || (stringEnumConverter != null);

            var describeInCamelCase = Options.DescribeStringEnumsInCamelCase
#if NETCOREAPP3_0
                || (stringEnumConverter != null && stringEnumConverter.NamingStrategy is CamelCaseNamingStrategy);
#else
                || (stringEnumConverter != null && stringEnumConverter.CamelCaseText);
#endif

            var enumType = jsonPrimitiveContract.UnderlyingType;
            var enumUnderlyingType = describeAsString ? typeof(string) : enumType.GetEnumUnderlyingType();

            var schema = FactoryMethodMap[enumUnderlyingType]();

            if (describeAsString)
            {
                schema.Enum = enumType.GetEnumNames()
                    .Distinct()
                    .Select(name =>
                    {
                        name = describeInCamelCase ? name.ToCamelCase() : name;
                        return (IOpenApiAny)(new OpenApiString(name));
                    })
                    .ToList();
            }
            else
            {
                schema.Enum = enumType.GetEnumValues()
                    .Cast<object>()
                    .Distinct()
                    .Select(value =>
                    {
                        value = Convert.ChangeType(value, enumUnderlyingType);
                        return OpenApiAnyFactory.TryCreateFor(schema, value, out IOpenApiAny openApiAny) ? openApiAny : null;
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
            { typeof(Uri), () => new OpenApiSchema { Type = "string", Format = "uri" } },
            { typeof(TimeSpan), () => new OpenApiSchema { Type = "string", Format = "date-span" } },
        };
    }
}
