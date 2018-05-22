using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class SchemaExtensions
    {
        internal static Schema AssignValidationProperties(this Schema schema, JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.MemberInfo();
            if (propInfo == null)
                return schema;

            foreach (var attribute in propInfo.GetCustomAttributes(false))
            {
                if (attribute is DefaultValueAttribute defaultValue)
                    schema.Default = defaultValue.Value;

                if (attribute is RegularExpressionAttribute regex)
                    schema.Pattern = regex.Pattern;

                if (attribute is RangeAttribute range)
                {
                    if (Int32.TryParse(range.Maximum.ToString(), out int maximum))
                        schema.Maximum = maximum;

                    if (Int32.TryParse(range.Minimum.ToString(), out int minimum))
                        schema.Minimum = minimum;
                }

                if (attribute is MinLengthAttribute minLength)
                    schema.MinLength = minLength.Length;

                if (attribute is MaxLengthAttribute maxLength)
                    schema.MaxLength = maxLength.Length;

                if (attribute is StringLengthAttribute stringLength)
                {
                    schema.MinLength = stringLength.MinimumLength;
                    schema.MaxLength = stringLength.MaximumLength;
                }

                if (attribute is DataTypeAttribute dataTypeAttribute && schema.Type == "string")
                {
                    if (DataTypeFormatMap.TryGetValue(dataTypeAttribute.DataType, out string format))
                        schema.Format = format;
                }
            }

            if (!jsonProperty.Writable)
                schema.ReadOnly = true;

            return schema;
        }

        internal static void PopulateFrom(this PartialSchema partialSchema, Schema schema)
        {
            if (schema == null) return;

            partialSchema.Type = schema.Type;
            partialSchema.Format = schema.Format;

            if (schema.Items != null)
            {
                // TODO: Handle jagged primitive array and error on jagged object array
                partialSchema.Items = new PartialSchema();
                partialSchema.Items.PopulateFrom(schema.Items);
            }

            partialSchema.Default = schema.Default;
            partialSchema.Maximum = schema.Maximum;
            partialSchema.ExclusiveMaximum = schema.ExclusiveMaximum;
            partialSchema.Minimum = schema.Minimum;
            partialSchema.ExclusiveMinimum = schema.ExclusiveMinimum;
            partialSchema.MaxLength = schema.MaxLength;
            partialSchema.MinLength = schema.MinLength;
            partialSchema.Pattern = schema.Pattern;
            partialSchema.MaxItems = schema.MaxItems;
            partialSchema.MinItems = schema.MinItems;
            partialSchema.UniqueItems = schema.UniqueItems;
            partialSchema.Enum = schema.Enum;
            partialSchema.MultipleOf = schema.MultipleOf;
        }

        private static readonly Dictionary<DataType, string> DataTypeFormatMap = new Dictionary<DataType, string>
        {
            { DataType.Date, "date" },
            { DataType.DateTime, "date-time" },
            { DataType.Password, "password" }
        };
    }
}
