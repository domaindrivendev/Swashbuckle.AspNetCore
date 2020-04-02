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
        public static void ApplyCustomAttributes(this OpenApiSchema schema, IEnumerable<object> customAttributes)
        {
            foreach (var attribute in customAttributes)
            {
                if (attribute is DefaultValueAttribute defaultValue && defaultValue.Value != null)
                {
                    schema.Default = OpenApiAnyFactory.CreateFor(schema, defaultValue.Value);
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
                else if (attribute is RequiredAttribute)
                {
                    schema.Nullable = false;
                }
                else if (attribute is ObsoleteAttribute)
                {
                    schema.Deprecated = true;
                }
            }
        }

        private static readonly Dictionary<AnnotationsDataType, string> DataTypeFormatMap = new Dictionary<AnnotationsDataType, string>
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
    }
}
