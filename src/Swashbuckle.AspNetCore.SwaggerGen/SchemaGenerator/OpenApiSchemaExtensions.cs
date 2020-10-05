using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using AnnotationsDataType = System.ComponentModel.DataAnnotations.DataType;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiSchemaExtensions
    {
        public static void ApplyCustomAttributes(this OpenApiSchema schema, IEnumerable<object> customAttributes) => ApplyCustomAttributes(schema, null, customAttributes);

        public static void ApplyCustomAttributes(this OpenApiSchema schema, SchemaRepository schemaRepository, IEnumerable<object> customAttributes)
        {
            foreach (var attribute in customAttributes)
            {
                if (attribute is CreditCardAttribute creditCardAttribute)
                    ApplyCreditCardAttribute(schema, creditCardAttribute);

                else if (attribute is DataTypeAttribute dataTypeAttribute)
                    ApplyDataTypeAttribute(schema, dataTypeAttribute);

                else if (attribute is DefaultValueAttribute defaultValueAttribute)
                    ApplyDefaultValueAttribute(schema, schemaRepository, defaultValueAttribute);

                else if (attribute is EmailAddressAttribute emailAddressAttribute)
                    ApplyEmailAddressAttribute(schema, emailAddressAttribute);

                else if (attribute is MaxLengthAttribute maxLengthAttribute)
                    ApplyMaxLengthAttribute(schema, maxLengthAttribute);

                else if (attribute is MinLengthAttribute minLengthAttribute)
                    ApplyMinLengthAttribute(schema, minLengthAttribute);

                else if (attribute is ObsoleteAttribute obsoleteAttribute)
                    ApplyObsoleteAttribute(schema, obsoleteAttribute);

                else if (attribute is PhoneAttribute phoneAttribute)
                    ApplyPhoneAttribute(schema, phoneAttribute);

                else if (attribute is RangeAttribute rangeAttribute)
                    ApplyRangeAttribute(schema, rangeAttribute);

                else if (attribute is RegularExpressionAttribute regularExpressionAttribute)
                    ApplyRegularExpressionAttribute(schema, regularExpressionAttribute);

                else if (attribute is RequiredAttribute requiredAttribute)
                    ApplyRequiredAttribute(schema, requiredAttribute);

                else if (attribute is StringLengthAttribute stringLengthAttribute)
                    ApplyStringLengthAttribute(schema, stringLengthAttribute);
            }
        }

        private static void ApplyCreditCardAttribute(OpenApiSchema schema, CreditCardAttribute creditCardAttribute)
        {
            schema.Format = "credit-card";
        }

        private static void ApplyDataTypeAttribute(OpenApiSchema schema, DataTypeAttribute dataTypeAttribute)
        {
            var formats = new Dictionary<AnnotationsDataType, string>
            {
                { AnnotationsDataType.Date, "date" },
                { AnnotationsDataType.DateTime, "date-time" },
                { AnnotationsDataType.Time, "time" },
                { AnnotationsDataType.Duration, "duration" },
                { AnnotationsDataType.PhoneNumber, "tel" },
                { AnnotationsDataType.Currency, "currency" },
                { AnnotationsDataType.Text, "string" },
                { AnnotationsDataType.Html, "html" },
                { AnnotationsDataType.MultilineText, "multiline" },
                { AnnotationsDataType.EmailAddress, "email" },
                { AnnotationsDataType.Password, "password" },
                { AnnotationsDataType.Url, "uri" },
                { AnnotationsDataType.ImageUrl, "uri" },
                { AnnotationsDataType.CreditCard, "credit-card" },
                { AnnotationsDataType.PostalCode, "postal-code" },
            };

            if (formats.TryGetValue(dataTypeAttribute.DataType, out string format))
            {
                schema.Format = format;
            }
        }

        private static void ApplyDefaultValueAttribute(OpenApiSchema schema, SchemaRepository schemaRepository, DefaultValueAttribute defaultValueAttribute)
        {
            schema.Default = OpenApiAnyFactory.CreateFor(schema, schemaRepository, defaultValueAttribute.Value);
        }

        private static void ApplyEmailAddressAttribute(OpenApiSchema schema, EmailAddressAttribute emailAddressAttribute)
        {
            schema.Format = "email";
        }

        private static void ApplyMaxLengthAttribute(OpenApiSchema schema, MaxLengthAttribute maxLengthAttribute)
        {
            if (schema.Type == "array")
                schema.MaxItems = maxLengthAttribute.Length;
            else
                schema.MaxLength = maxLengthAttribute.Length;
        }

        private static void ApplyMinLengthAttribute(OpenApiSchema schema, MinLengthAttribute minLengthAttribute)
        {
            if (schema.Type == "array")
                schema.MinItems = minLengthAttribute.Length;
            else
                schema.MinLength = minLengthAttribute.Length;
        }

        private static void ApplyObsoleteAttribute(OpenApiSchema schema, ObsoleteAttribute obsoleteAttribute)
        {
            schema.Deprecated = true;
        }

        private static void ApplyPhoneAttribute(OpenApiSchema schema, PhoneAttribute phoneAttribute)
        {
            schema.Format = "tel";
        }

        private static void ApplyRangeAttribute(OpenApiSchema schema, RangeAttribute rangeAttribute)
        {
            schema.Maximum = decimal.TryParse(rangeAttribute.Maximum.ToString(), out decimal maximum)
                ? maximum
                : schema.Maximum;

            schema.Minimum = decimal.TryParse(rangeAttribute.Minimum.ToString(), out decimal minimum)
                ? minimum
                : schema.Minimum;
        }

        private static void ApplyRegularExpressionAttribute(OpenApiSchema schema, RegularExpressionAttribute regularExpressionAttribute)
        {
            schema.Pattern = regularExpressionAttribute.Pattern;
        }

        private static void ApplyRequiredAttribute(OpenApiSchema schema, RequiredAttribute requiredAttribute)
        {
            schema.Nullable = false;
        }

        private static void ApplyStringLengthAttribute(OpenApiSchema schema, StringLengthAttribute stringLengthAttribute)
        {
            schema.MinLength = stringLengthAttribute.MinimumLength;
            schema.MaxLength = stringLengthAttribute.MaximumLength;
        }
    }
}
