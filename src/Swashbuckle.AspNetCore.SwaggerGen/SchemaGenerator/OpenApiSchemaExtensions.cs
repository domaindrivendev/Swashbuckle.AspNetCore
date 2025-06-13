using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using AnnotationsDataType = System.ComponentModel.DataAnnotations.DataType;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public static class OpenApiSchemaExtensions
{
    private static readonly Dictionary<AnnotationsDataType, string> DataFormatMappings = new()
    {
        [AnnotationsDataType.DateTime] = "date-time",
        [AnnotationsDataType.Date] = "date",
        [AnnotationsDataType.Time] = "time",
        [AnnotationsDataType.Duration] = "duration",
        [AnnotationsDataType.PhoneNumber] = "tel",
        [AnnotationsDataType.Currency] = "currency",
        [AnnotationsDataType.Text] = "string",
        [AnnotationsDataType.Html] = "html",
        [AnnotationsDataType.MultilineText] = "multiline",
        [AnnotationsDataType.EmailAddress] = "email",
        [AnnotationsDataType.Password] = "password",
        [AnnotationsDataType.Url] = "uri",
        [AnnotationsDataType.ImageUrl] = "uri",
        [AnnotationsDataType.CreditCard] = "credit-card",
        [AnnotationsDataType.PostalCode] = "postal-code",
        [AnnotationsDataType.Upload] = "binary",
    };

    public static void ApplyValidationAttributes(this IOpenApiSchema schema, IEnumerable<object> customAttributes)
    {
        if (schema is OpenApiSchema concrete)
        {
            ApplyValidationAttributes(concrete, customAttributes);
        }
    }

    private static void ApplyValidationAttributes(OpenApiSchema schema, IEnumerable<object> customAttributes)
    {
        foreach (var attribute in customAttributes)
        {
            if (attribute is DataTypeAttribute dataTypeAttribute)
            {
                ApplyDataTypeAttribute(schema, dataTypeAttribute);
            }
            else if (attribute is MinLengthAttribute minLengthAttribute)
            {
                ApplyMinLengthAttribute(schema, minLengthAttribute);
            }
            else if (attribute is MaxLengthAttribute maxLengthAttribute)
            {
                ApplyMaxLengthAttribute(schema, maxLengthAttribute);
            }
            else if (attribute is LengthAttribute lengthAttribute)
            {
                ApplyLengthAttribute(schema, lengthAttribute);
            }
            else if (attribute is Base64StringAttribute base64Attribute)
            {
                ApplyBase64Attribute(schema);
            }
            else if (attribute is RangeAttribute rangeAttribute)
            {
                ApplyRangeAttribute(schema, rangeAttribute);
            }
            else if (attribute is RegularExpressionAttribute regularExpressionAttribute)
            {
                ApplyRegularExpressionAttribute(schema, regularExpressionAttribute);
            }
            else if (attribute is StringLengthAttribute stringLengthAttribute)
            {
                ApplyStringLengthAttribute(schema, stringLengthAttribute);
            }
            else if (attribute is ReadOnlyAttribute readOnlyAttribute)
            {
                ApplyReadOnlyAttribute(schema, readOnlyAttribute);
            }
            else if (attribute is DescriptionAttribute descriptionAttribute)
            {
                ApplyDescriptionAttribute(schema, descriptionAttribute);
            }
        }
    }

    public static void ApplyRouteConstraints(this IOpenApiSchema schema, ApiParameterRouteInfo routeInfo)
    {
        if (schema is OpenApiSchema concrete)
        {
            ApplyRouteConstraints(concrete, routeInfo);
        }
    }

    private static void ApplyRouteConstraints(OpenApiSchema schema, ApiParameterRouteInfo routeInfo)
    {
        foreach (var constraint in routeInfo.Constraints)
        {
            if (constraint is MinRouteConstraint minRouteConstraint)
            {
                ApplyMinRouteConstraint(schema, minRouteConstraint);
            }
            else if (constraint is MaxRouteConstraint maxRouteConstraint)
            {
                ApplyMaxRouteConstraint(schema, maxRouteConstraint);
            }
            else if (constraint is MinLengthRouteConstraint minLengthRouteConstraint)
            {
                ApplyMinLengthRouteConstraint(schema, minLengthRouteConstraint);
            }
            else if (constraint is MaxLengthRouteConstraint maxLengthRouteConstraint)
            {
                ApplyMaxLengthRouteConstraint(schema, maxLengthRouteConstraint);
            }
            else if (constraint is RangeRouteConstraint rangeRouteConstraint)
            {
                ApplyRangeRouteConstraint(schema, rangeRouteConstraint);
            }
            else if (constraint is RegexRouteConstraint regexRouteConstraint)
            {
                ApplyRegexRouteConstraint(schema, regexRouteConstraint);
            }
            else if (constraint is LengthRouteConstraint lengthRouteConstraint)
            {
                ApplyLengthRouteConstraint(schema, lengthRouteConstraint);
            }
            else if (constraint is FloatRouteConstraint or DecimalRouteConstraint)
            {
                schema.Type = JsonSchemaTypes.Number;
            }
            else if (constraint is LongRouteConstraint or IntRouteConstraint)
            {
                schema.Type = JsonSchemaTypes.Integer;
            }
            else if (constraint is GuidRouteConstraint or StringRouteConstraint)
            {
                schema.Type = JsonSchemaTypes.String;
            }
            else if (constraint is BoolRouteConstraint)
            {
                schema.Type = JsonSchemaTypes.Boolean;
            }
        }
    }

    internal static JsonSchemaType? ResolveType(this IOpenApiSchema schema, SchemaRepository schemaRepository)
    {
        if (schema is OpenApiSchemaReference reference &&
            schemaRepository.Schemas.TryGetValue(reference.Reference.Id, out var definitionSchema))
        {
            return definitionSchema.ResolveType(schemaRepository);
        }

        if (schema.AllOf is { Count: > 0 } allOf)
        {
            foreach (var subSchema in allOf)
            {
                var type = subSchema.ResolveType(schemaRepository);
                if (type != null)
                {
                    return type;
                }
            }
        }

        return schema.Type;
    }

    private static void ApplyDataTypeAttribute(OpenApiSchema schema, DataTypeAttribute dataTypeAttribute)
    {
        if (DataFormatMappings.TryGetValue(dataTypeAttribute.DataType, out string format))
        {
            schema.Format = format;
        }
    }

    private static void ApplyMinLengthAttribute(OpenApiSchema schema, MinLengthAttribute minLengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = minLengthAttribute.Length;
        }
        else
        {
            schema.MinLength = minLengthAttribute.Length;
        }
    }

    private static void ApplyMinLengthRouteConstraint(OpenApiSchema schema, MinLengthRouteConstraint minLengthRouteConstraint)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = minLengthRouteConstraint.MinLength;
        }
        else
        {
            schema.MinLength = minLengthRouteConstraint.MinLength;
        }
    }

    private static void ApplyMaxLengthAttribute(OpenApiSchema schema, MaxLengthAttribute maxLengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MaxItems = maxLengthAttribute.Length;
        }
        else
        {
            schema.MaxLength = maxLengthAttribute.Length;
        }
    }

    private static void ApplyMaxLengthRouteConstraint(OpenApiSchema schema, MaxLengthRouteConstraint maxLengthRouteConstraint)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MaxItems = maxLengthRouteConstraint.MaxLength;
        }
        else
        {
            schema.MaxLength = maxLengthRouteConstraint.MaxLength;
        }
    }

    private static void ApplyLengthAttribute(OpenApiSchema schema, LengthAttribute lengthAttribute)
    {
        if (schema.Type is { } type && type.HasFlag(JsonSchemaTypes.Array))
        {
            schema.MinItems = lengthAttribute.MinimumLength;
            schema.MaxItems = lengthAttribute.MaximumLength;
        }
        else
        {
            schema.MinLength = lengthAttribute.MinimumLength;
            schema.MaxLength = lengthAttribute.MaximumLength;
        }
    }

    private static void ApplyBase64Attribute(OpenApiSchema schema)
    {
        schema.Format = "byte";
    }

    private static void ApplyRangeAttribute(OpenApiSchema schema, RangeAttribute rangeAttribute)
    {
        object maximumValue = null;
        object minimumValue = null;

        if (rangeAttribute.Maximum is double || rangeAttribute.Minimum is int)
        {
            // The range was set with the RangeAttribute(double, double) or RangeAttribute(int, int)
            // constructor so we can safely convert the values to strings using the invariant culture
            // as we have primitive values to operate on.
            maximumValue = rangeAttribute.Maximum;
            minimumValue = rangeAttribute.Minimum;
        }
        else
        {
            // Parse the range from the RangeAttribute(double, double) or RangeAttribute(string, string) constructor.
            // Use the appropriate culture as the user may have specified a culture-specific format for the numbers
            // if they specified the value as a string. By default RangeAttribute uses the current culture, but it
            // can be set to use the invariant culture.
            var targetCulture = rangeAttribute.ParseLimitsInInvariantCulture || rangeAttribute.Minimum is double
                ? CultureInfo.InvariantCulture
                : CultureInfo.CurrentCulture;

            var maxString = Convert.ToString(rangeAttribute.Maximum, targetCulture);
            var minString = Convert.ToString(rangeAttribute.Minimum, targetCulture);

            if (decimal.TryParse(maxString, NumberStyles.Any, targetCulture, out var value))
            {
                maximumValue = value;
            }

            if (decimal.TryParse(minString, NumberStyles.Any, targetCulture, out value))
            {
                minimumValue = value;
            }
        }

        // Ensure that the conversion to string is done using the invariant culture so valid JSON is generated
        if (maximumValue is not null)
        {
            schema.Maximum = Convert.ToString(maximumValue, CultureInfo.InvariantCulture);
        }

        if (minimumValue is not null)
        {
            schema.Minimum = Convert.ToString(minimumValue, CultureInfo.InvariantCulture);
        }

        if (rangeAttribute.MaximumIsExclusive)
        {
            schema.ExclusiveMaximum = schema.Maximum;
        }

        if (rangeAttribute.MinimumIsExclusive)
        {
            schema.ExclusiveMinimum = schema.Minimum;
        }
    }

    private static void ApplyRangeRouteConstraint(OpenApiSchema schema, RangeRouteConstraint rangeRouteConstraint)
    {
        schema.Maximum = rangeRouteConstraint.Max.ToString(CultureInfo.InvariantCulture);
        schema.Minimum = rangeRouteConstraint.Min.ToString(CultureInfo.InvariantCulture);
    }

    private static void ApplyMinRouteConstraint(OpenApiSchema schema, MinRouteConstraint minRouteConstraint)
        => schema.Minimum = minRouteConstraint.Min.ToString(CultureInfo.InvariantCulture);

    private static void ApplyMaxRouteConstraint(OpenApiSchema schema, MaxRouteConstraint maxRouteConstraint)
        => schema.Maximum = maxRouteConstraint.Max.ToString(CultureInfo.InvariantCulture);

    private static void ApplyRegularExpressionAttribute(OpenApiSchema schema, RegularExpressionAttribute regularExpressionAttribute)
    {
        schema.Pattern = regularExpressionAttribute.Pattern;
    }

    private static void ApplyRegexRouteConstraint(OpenApiSchema schema, RegexRouteConstraint regexRouteConstraint)
        => schema.Pattern = regexRouteConstraint.Constraint.ToString();

    private static void ApplyStringLengthAttribute(OpenApiSchema schema, StringLengthAttribute stringLengthAttribute)
    {
        schema.MinLength = stringLengthAttribute.MinimumLength;
        schema.MaxLength = stringLengthAttribute.MaximumLength;
    }

    private static void ApplyReadOnlyAttribute(OpenApiSchema schema, ReadOnlyAttribute readOnlyAttribute)
    {
        schema.ReadOnly = readOnlyAttribute.IsReadOnly;
    }

    private static void ApplyDescriptionAttribute(OpenApiSchema schema, DescriptionAttribute descriptionAttribute)
    {
        schema.Description ??= descriptionAttribute.Description;
    }

    private static void ApplyLengthRouteConstraint(OpenApiSchema schema, LengthRouteConstraint lengthRouteConstraint)
    {
        schema.MinLength = lengthRouteConstraint.MinLength;
        schema.MaxLength = lengthRouteConstraint.MaxLength;
    }
}
