using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
{
    internal static class SchemaExtensions
    {
        internal static Schema AssignValidationProperties(this Schema schema, JsonProperty jsonProperty)
        {
            var propInfo = jsonProperty.PropertyInfo();
            if (propInfo == null)
                return schema;

            foreach (var attribute in propInfo.GetCustomAttributes(false))
            {
                var regex = attribute as RegularExpressionAttribute;
                if (regex != null)
                    schema.Pattern = regex.Pattern;

                var range = attribute as RangeAttribute;
                if (range != null)
                {
                    int maximum;
                    if (Int32.TryParse(range.Maximum.ToString(), out maximum))
                        schema.Maximum = maximum;

                    int minimum;
                    if (Int32.TryParse(range.Minimum.ToString(), out minimum))
                        schema.Minimum = minimum;
                }

                var minLength = attribute as MinLengthAttribute;
                if (minLength != null)
                    schema.MinLength = minLength.Length;

                var maxLength = attribute as MaxLengthAttribute;
                if (maxLength != null)
                    schema.MaxLength = maxLength.Length;

                var stringLength = attribute as StringLengthAttribute;
                if (stringLength != null)
                {
                    schema.MinLength = stringLength.MinimumLength;
                    schema.MaxLength = stringLength.MaximumLength;
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
    }
}
