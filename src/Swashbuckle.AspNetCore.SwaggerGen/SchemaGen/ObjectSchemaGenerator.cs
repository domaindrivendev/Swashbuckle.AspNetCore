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

                properties.Add(jsonProperty.PropertyName, GeneratePropertySchema(jsonProperty, memberInfo, attributes, schemaRepository));

                if (jsonProperty.Required == Required.AllowNull || jsonProperty.Required == Required.Always || attributes.OfType<RequiredAttribute>().Any())
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
            MemberInfo memberInfo,
            object[] attributes,
            SchemaRepository schemaRepository) 
        {
            var schema = RootGenerator.GenerateSchema(jsonProperty.PropertyType, schemaRepository);

            schema.WriteOnly = jsonProperty.Writable && !jsonProperty.Readable;
            schema.ReadOnly = jsonProperty.Readable && !jsonProperty.Writable;

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
                else if (attribute is EmailAddressAttribute)
                {
                    schema.Format = "email";
                }
                else if (attribute is CreditCardAttribute)
                {
                    schema.Format = "credit-card";
                }
                else if (attribute is PhoneAttribute)
                {
                    schema.Format = "tel";
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

        private static readonly Dictionary<DataType, string> DataTypeFormatMap = new Dictionary<DataType, string>
        {
            { DataType.Date, "date" },
            { DataType.DateTime, "date-time" },
            { DataType.Time, "time" },
            { DataType.Duration, "duration" },
            { DataType.PhoneNumber, "tel" },
            { DataType.Currency, "currency" },
            { DataType.Text, "string" },
            { DataType.Html, "html" },
            { DataType.MultilineText, "multiline" },
            { DataType.EmailAddress, "email" },
            { DataType.Password, "password" },
            { DataType.Url, "uri" },
            { DataType.ImageUrl, "uri" },
            { DataType.CreditCard, "credit-card" },
            { DataType.PostalCode, "postal-code" },
        };
    }
}