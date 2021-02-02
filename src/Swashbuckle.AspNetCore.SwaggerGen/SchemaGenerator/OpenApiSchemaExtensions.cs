using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using AnnotationsDataType = System.ComponentModel.DataAnnotations.DataType;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class OpenApiSchemaExtensions
    {
        public static void ApplyValidationAttributes(this OpenApiSchema schema, IEnumerable<object> customAttributes)
        {
            foreach (var attribute in customAttributes)
            {
                if (attribute is DataTypeAttribute dataTypeAttribute)
                    ApplyDataTypeAttribute(schema, dataTypeAttribute);

                else if (attribute is MinLengthAttribute minLengthAttribute)
                    ApplyMinLengthAttribute(schema, minLengthAttribute);

                else if (attribute is MaxLengthAttribute maxLengthAttribute)
                    ApplyMaxLengthAttribute(schema, maxLengthAttribute);

                else if (attribute is RangeAttribute rangeAttribute)
                    ApplyRangeAttribute(schema, rangeAttribute);

                else if (attribute is RegularExpressionAttribute regularExpressionAttribute)
                    ApplyRegularExpressionAttribute(schema, regularExpressionAttribute);

                else if (attribute is StringLengthAttribute stringLengthAttribute)
                    ApplyStringLengthAttribute(schema, stringLengthAttribute);
            }
        }

        public static string ResolveType(this OpenApiSchema schema, SchemaRepository schemaRepository)
        {
            if (schema.Reference != null && schemaRepository.Schemas.TryGetValue(schema.Reference.Id, out OpenApiSchema definitionSchema))
                return definitionSchema.ResolveType(schemaRepository);

            foreach (var subSchema in schema.AllOf)
            {
                var type = subSchema.ResolveType(schemaRepository);
                if (type != null) return type;
            }

            return schema.Type;
        }

        private static void ApplyDataTypeAttribute(OpenApiSchema schema, DataTypeAttribute dataTypeAttribute)
        {
            var formats = new Dictionary<AnnotationsDataType, string>
            {
                { AnnotationsDataType.DateTime, "date-time" },
                { AnnotationsDataType.Date, "date" },
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
                { AnnotationsDataType.PostalCode, "postal-code" }
            };

            if (formats.TryGetValue(dataTypeAttribute.DataType, out string format))
            {
                schema.Format = format;
            }
        }

        private static void ApplyMinLengthAttribute(OpenApiSchema schema, MinLengthAttribute minLengthAttribute)
        {
            if (schema.Type == "array")
                schema.MinItems = minLengthAttribute.Length;
            else
                schema.MinLength = minLengthAttribute.Length;
        }

        private static void ApplyMaxLengthAttribute(OpenApiSchema schema, MaxLengthAttribute maxLengthAttribute)
        {
            if (schema.Type == "array")
                schema.MaxItems = maxLengthAttribute.Length;
            else
                schema.MaxLength = maxLengthAttribute.Length;
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

        private static void ApplyStringLengthAttribute(OpenApiSchema schema, StringLengthAttribute stringLengthAttribute)
        {
            schema.MinLength = stringLengthAttribute.MinimumLength;
            schema.MaxLength = stringLengthAttribute.MaximumLength;
        }
    }
}
