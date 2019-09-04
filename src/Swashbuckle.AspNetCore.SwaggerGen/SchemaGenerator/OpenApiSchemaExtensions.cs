using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiSchemaExtensions
    {
        public static void ApplyCustomAttributes(this OpenApiSchema schema, IEnumerable<object> customAttributes)
        {
            foreach (var attribute in customAttributes)
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
