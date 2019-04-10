using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ObjectSchemaGenerator : ChainableSchemaGenerator
    {
        public ObjectSchemaGenerator(
            IContractResolver contractResolver,
            ISchemaGenerator rootGenerator,
            SchemaGeneratorOptions options)
            : base(contractResolver, rootGenerator, options)
        { }

        protected override bool CanGenerateSchemaFor(Type type)
        {
            return ContractResolver.ResolveContract(type) is JsonObjectContract;
        }

        protected override OpenApiSchema GenerateSchemaFor(Type type, SchemaRepository schemaRepository)
        {
            var jsonObjectContract = (JsonObjectContract)ContractResolver.ResolveContract(type);

            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();

            foreach (var jsonProperty in jsonObjectContract.Properties)
            {
                if (jsonProperty.Ignored) continue;

                var attributes = jsonProperty.TryGetMemberInfo(out MemberInfo memberInfo)
                    ? memberInfo.GetCustomAttributes(true)
                    : new object[] { };

                if (Options.IgnoreObsoleteProperties && attributes.OfType<ObsoleteAttribute>().Any()) continue;

                var required = GetRequiredValue(jsonProperty, jsonObjectContract, memberInfo);
                properties.Add(jsonProperty.PropertyName, GeneratePropertySchema(jsonProperty, required, memberInfo, attributes, schemaRepository));

                if (required == Required.AllowNull || required == Required.Always || attributes.OfType<RequiredAttribute>().Any())
                    requiredPropertyNames.Add(jsonProperty.PropertyName);
            }

            var additionalProperties = (jsonObjectContract.ExtensionDataValueType != null)
                ? RootGenerator.GenerateSchema(jsonObjectContract.ExtensionDataValueType, schemaRepository)
                : null;

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames),
                AdditionalPropertiesAllowed = (additionalProperties != null),
                AdditionalProperties = additionalProperties
            };

            return schema;
        }

        private OpenApiSchema GeneratePropertySchema(
            JsonProperty jsonProperty,
            Required? required,
            MemberInfo memberInfo,
            object[] attributes,
            SchemaRepository schemaRepository) 
        {
            var schema = RootGenerator.GenerateSchema(jsonProperty.PropertyType, schemaRepository);

            schema.WriteOnly = jsonProperty.Writable && !jsonProperty.Readable;
            schema.ReadOnly = jsonProperty.Readable && !jsonProperty.Writable;

            if (required == null)
            {
                // No json attributes -> default for nullable types is true.
                schema.Nullable = jsonProperty.PropertyType.IsNullable() || jsonProperty.PropertyType.IsFSharpOption();
            }
            else
            {
                schema.Nullable = required == Required.AllowNull || required == Required.Default;
            }

            foreach (var attribute in attributes)
            {
                if (attribute is DefaultValueAttribute defaultValue)
                {
                    schema.Default = OpenApiAnyFactory.TryCreateFor(schema, defaultValue.Value, out IOpenApiAny openApiAny)
                        ? openApiAny
                        : schema.Default;
                }
                else if (attribute is RegularExpressionAttribute regex)
                {
                    schema.Pattern = regex.Pattern;
                }
                else if (attribute is RangeAttribute range)
                {
                    schema.Maximum = decimal.TryParse(range.Maximum.ToString(), out decimal maximum)
                        ? maximum
                        : schema.Maximum;

                    schema.Minimum = decimal.TryParse(range.Minimum.ToString(), out decimal minimum)
                        ? minimum
                        : schema.Minimum;
                }
                else if (attribute is MinLengthAttribute minLength)
                {
                    schema.MinLength = minLength.Length;
                }
                else if (attribute is MaxLengthAttribute maxLength)
                {
                    schema.MaxLength = maxLength.Length;
                }
                else if (attribute is StringLengthAttribute stringLength)
                {
                    schema.MinLength = stringLength.MinimumLength;
                    schema.MaxLength = stringLength.MaximumLength;
                }
                else if (attribute is DataTypeAttribute dataTypeAttribute && schema.Type == "string")
                {
                    schema.Format = DataTypeFormatMap.TryGetValue(dataTypeAttribute.DataType, out string format)
                        ? format
                        : schema.Format;
                }
            }

            return schema;
        }
        /// <summary>
        /// Gets the <seealso cref="Required"/> value for the specified json property.
        /// </summary>
        /// <param name="jsonObject">Json object.</param>
        /// <param name="jsonProperty">Json property.</param>
        /// <param name="memberInfo">Member info.</param>
        /// <returns><seealso cref="Required"/> value.</returns>
        private Required? GetRequiredValue(JsonProperty jsonProperty, JsonObjectContract jsonObject, MemberInfo memberInfo)
        {
            if (jsonProperty.Required != Required.Default)
                return jsonProperty.Required;

            // Special handling only required for Required.Default.
            // Check if Required.Default is actually specified or if it is just the default value.
            // Those checks are needed to mirror the behaviour of newtonsoft json.
            var requiredType = typeof(Required);
            var jsonPropType = typeof(JsonPropertyAttribute);
            var attributeData = memberInfo.GetCustomAttributeData(jsonPropType, true);
            if (attributeData == null)
            {
                // No JsonPropertyAttribute found. Returns null if no JsonObjectAttribute is specified.
                return jsonObject.ItemRequired;
            }
            // Check if the attribute is set on the property and the value for required is explicitly specified.
            else if (attributeData.NamedArguments.Any(c => c.TypedValue.ArgumentType == requiredType))
            {
                return jsonProperty.Required;
            }
            // JsonProperty attribute is specified - don't return null.
            return jsonObject.ItemRequired ?? Required.Default;
        }

        private static readonly Dictionary<DataType, string> DataTypeFormatMap = new Dictionary<DataType, string>
        {
            { DataType.Date, "date" },
            { DataType.DateTime, "date-time" },
            { DataType.Password, "password" }
        };
    }
}