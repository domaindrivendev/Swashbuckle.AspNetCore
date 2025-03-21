using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using Xunit;

using JsonSchemaType = string;

namespace Swashbuckle.AspNetCore.ApiTesting.Test;

public class JsonValidatorTests
{
    public static TheoryData<JsonSchemaType, string, bool, string> Validate_ReturnsError_IfInstanceNotOfExpectedTypeData =>
        new()
        {
            { JsonSchemaTypes.Null, "{}", false, "Path: . Instance is not of type 'null'" },
            { JsonSchemaTypes.Null, "null", true, null },
            { JsonSchemaTypes.Boolean, "'foobar'", false, "Path: . Instance is not of type 'boolean'" },
            { JsonSchemaTypes.Boolean, "true", true, null },
            { JsonSchemaTypes.Object, "'foobar'", false, "Path: . Instance is not of type 'object'" },
            { JsonSchemaTypes.Object, "{}", true, null },
            { JsonSchemaTypes.Array, "'foobar'", false, "Path: . Instance is not of type 'array'" },
            { JsonSchemaTypes.Array, "[]", true, null },
            { JsonSchemaTypes.Number, "'foobar'", false, "Path: . Instance is not of type 'number'" },
            { JsonSchemaTypes.Number, "1", true, null },
            { JsonSchemaTypes.String, "{}", false, "Path: . Instance is not of type 'string'" },
            { JsonSchemaTypes.String, "'foobar'", true, null },
        };

    [Theory]
    [MemberData(nameof(Validate_ReturnsError_IfInstanceNotOfExpectedTypeData))]
    public void Validate_ReturnsError_IfInstanceNotOfExpectedType(
        JsonSchemaType schemaType,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema { Type = schemaType };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(5.0, "9", false, "Path: . Number is not evenly divisible by multipleOf")]
    [InlineData(5.0, "10", true, null)]
    public void Validate_ReturnsError_IfNumberNotEvenlyDivisibleByMultipleOf(
        decimal schemaMultipleOf,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema { Type = JsonSchemaTypes.Number, MultipleOf = schemaMultipleOf };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(10.0, "10.1", false, "Path: . Number is greater than maximum")]
    [InlineData(10.0, "10.0", true, null)]
    public void Validate_ReturnsError_IfNumberGreaterThanMaximum(
        decimal schemaMaximum,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema { Type = JsonSchemaTypes.Number, Maximum = schemaMaximum };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(10.0, "10.0", false, "Path: . Number is greater than, or equal to, maximum")]
    [InlineData(10.0, "9.9", true, null)]
    public void Validate_ReturnsError_IfNumberGreaterThanOrEqualToMaximumAndExclusiveMaximumSet(
        decimal schemaMaximum,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Number,
            Maximum = schemaMaximum,
            ExclusiveMaximum = true
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(10.0, "9.9", false, "Path: . Number is less than minimum")]
    [InlineData(10.0, "10.0", true, null)]
    public void Validate_ReturnsError_IfNumberLessThanMinimum(
        decimal schemaMinimum,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema { Type = JsonSchemaTypes.Number, Minimum = schemaMinimum };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(10.0, "10.0", false, "Path: . Number is less than, or equal to, minimum")]
    [InlineData(10.0, "10.1", true, null)]
    public void Validate_ReturnsError_IfNumberLessThanOrEqualToMinimumAndExclusiveMinimumSet(
        decimal schemaMinimum,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Number,
            Minimum = schemaMinimum,
            ExclusiveMinimum = true
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(5, "'123456'", false, "Path: . String length is greater than maxLength")]
    [InlineData(5, "'12345'", true, null)]
    public void Validate_ReturnsError_IfStringLengthGreaterThanMaxLength(
        int schemaMaxLength,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.String,
            MaxLength = schemaMaxLength
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(5, "'1234'", false, "Path: . String length is less than minLength" )]
    [InlineData(5, "'12345'", true, null)]
    public void Validate_ReturnsError_IfStringLengthLessThanMinLength(
        int schemaMinLength,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.String,
            MinLength = schemaMinLength
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("^[a-z]{3}$", "'aa1'", false, "Path: . String does not match pattern")]
    [InlineData("^[a-z]{3}$", "'aaz'", true, null)]
    public void Validate_ReturnsError_IfStringDoesNotMatchPattern(
        string schemaPattern,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.String,
            Pattern = schemaPattern
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    public static TheoryData<JsonSchemaType, string, bool, string> Validate_ReturnsError_IfArrayItemDoesNotMatchItemsSchemaData =>
        new()
        {
            { JsonSchemaTypes.Boolean, "[ true, 'foo' ]", false, "Path: [1]. Instance is not of type 'boolean'" },
            { JsonSchemaTypes.Number, "[ 123, 'foo' ]", false, "Path: [1]. Instance is not of type 'number'" },
            { JsonSchemaTypes.Boolean, "[ true, false ]", true, null },
        };

    [Theory]
    [MemberData(nameof(Validate_ReturnsError_IfArrayItemDoesNotMatchItemsSchemaData))]
    public void Validate_ReturnsError_IfArrayItemDoesNotMatchItemsSchema(
        JsonSchemaType itemsSchemaType,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Array,
            Items = new OpenApiSchema { Type = itemsSchemaType }
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(2, "[ 1, 2, 3 ]", false, "Path: . Array size is greater than maxItems")]
    [InlineData(2, "[ 1, 2 ]", true, null)]
    public void Validate_ReturnsError_IfArraySizeGreaterThanMaxItems(
        int schemaMaxItems,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Array,
            MaxItems = schemaMaxItems
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(2, "[ 1 ]", false, "Path: . Array size is less than minItems")]
    [InlineData(2, "[ 1, 2 ]", true, null)]
    public void Validate_ReturnsError_IfArraySizeLessThanMinItems(
        int schemaMinItems,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Array,
            MinItems = schemaMinItems
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("[ 1, 1, 3 ]", false, "Path: . Array does not contain uniqueItems")]
    [InlineData("[ 1, 2, 3 ]", true, null)]
    public void Validate_ReturnsError_IfArrayDoesNotContainUniqueItemsAndUniqueItemsSet(
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Array,
            UniqueItems = true
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(1, "{ \"id\": 1, \"name\": \"foo\" }", false, "Path: . Number of properties is greater than maxProperties")]
    [InlineData(1, "{ \"id\": 1 }", true, null)]
    public void Validate_ReturnsError_IfNumberOfPropertiesGreaterThanMaxProperties(
        int schemaMaxProperties,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            MaxProperties = schemaMaxProperties
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(2, "{ \"id\": 1 }", false, "Path: . Number of properties is less than minProperties")]
    [InlineData(2, "{ \"id\": 1, \"name\": \"foo\" }", true, null)]
    public void Validate_ReturnsError_IfNumberOfPropertiesLessThanMinProperties(
        int schemaMinProperties,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            MinProperties = schemaMinProperties
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(new[] { "id", "name" }, "{ \"id\": 1 }", false, "Path: . Required property(s) not present")]
    [InlineData(new[] { "id", "name" }, "{ \"id\": 1, \"name\": \"foo\" }", true, null)]
    public void Validate_ReturnsError_IfRequiredPropertyNotPresent(
        string[] schemaRequired,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            Required = new SortedSet<string>(schemaRequired)
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    public static TheoryData<JsonSchemaType, string, bool, string> Validate_ReturnsError_IfKnownPropertyDoesNotMatchPropertySchemaData =>
        new()
        {
            { JsonSchemaTypes.Number, "{ \"id\": \"foo\" }", false, "Path: id. Instance is not of type 'number'" },
            { JsonSchemaTypes.String, "{ \"id\": 123 }", false, "Path: id. Instance is not of type 'string'" },
            { JsonSchemaTypes.Number, "{ \"id\": 123 }", true, null },
        };

    [Theory]
    [MemberData(nameof(Validate_ReturnsError_IfKnownPropertyDoesNotMatchPropertySchemaData))]
    public void Validate_ReturnsError_IfKnownPropertyDoesNotMatchPropertySchema(
        JsonSchemaType propertySchemaType,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            Properties = new Dictionary<string, OpenApiSchema>
            {
                [ "id" ] = new OpenApiSchema { Type = propertySchemaType }
            }
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    public static TheoryData<JsonSchemaType, string, bool, string> Validate_ReturnsError_IfAdditionalPropertyDoesNotMatchAdditionalPropertiesSchemaData =>
        new()
        {
            { JsonSchemaTypes.Number, "{ \"id\": \"foo\" }", false, "Path: id. Instance is not of type 'number'" },
            { JsonSchemaTypes.String, "{ \"name\": 123 }", false, "Path: name. Instance is not of type 'string'" },
            { JsonSchemaTypes.Number, "{ \"description\": 123 }", true, null },
        };

    [Theory]
    [MemberData(nameof(Validate_ReturnsError_IfAdditionalPropertyDoesNotMatchAdditionalPropertiesSchemaData))]
    public void Validate_ReturnsError_IfAdditionalPropertyDoesNotMatchAdditionalPropertiesSchema(
        JsonSchemaType additionalPropertiesType,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            AdditionalProperties = new OpenApiSchema { Type = additionalPropertiesType }
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData(false, "{ \"id\": \"foo\" }", false, "Path: . Additional properties not allowed")]
    [InlineData(true, "{ \"id\": \"foo\" }", true, null)]
    public void Validate_ReturnsError_IfAdditionalPropertiesPresentAndAdditionalPropertiesAllowedUnset(
        bool additionalPropertiesAllowed,
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Type = JsonSchemaTypes.Object,
            AdditionalPropertiesAllowed = additionalPropertiesAllowed
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("{ \"p1\": 1, \"p2\": 2 }", false, "Path: . Required property(s) not present (allOf[2])")]
    [InlineData("{ \"p1\": 1, \"p2\": 2, \"p3\": 3 }", true, null)]
    public void Validate_ReturnsError_IfInstanceDoesNotMatchAllSchemasSpecifiedByAllOf(
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            AllOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p1" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p2" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p3" } }
            ]
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("{}", false, "Path: . Required property(s) not present (anyOf[0])")]
    [InlineData("{ \"p1\": 1 }", true, null)]
    public void Validate_ReturnsError_IfInstanceDoesNotMatchAnySchemaSpecifiedByAnyOf(
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            AnyOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p1" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p2" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p3" } }
            ]
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("{}", false, "Path: . Required property(s) not present (oneOf[0])")]
    [InlineData("{ \"p1\": 1, \"p2\": 2 }", false, "Path: . Instance matches multiple schemas in oneOf array")]
    [InlineData("{ \"p1\": 1 }", true, null)]
    public void Validate_ReturnsError_IfInstanceDoesNotMatchExactlyOneSchemaSpecifiedByOneOf(
        string instanceText,
        bool expectedReturnValue,
        string expectedErrorMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            OneOf =
            [
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p1" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p2" } },
                new OpenApiSchema { Type = JsonSchemaTypes.Object, Required = new SortedSet<string> { "p3" } }
            ]
        };
        var instance = JToken.Parse(instanceText);

        var returnValue = Subject().Validate(
            openApiSchema,
            new OpenApiDocument(),
            instance,
            out IEnumerable<string> errorMessages);

        Assert.Equal(expectedReturnValue, returnValue);
        Assert.Equal(expectedErrorMessage, errorMessages.FirstOrDefault());
    }

    [Theory]
    [InlineData("foo", "Invalid Reference identifier 'foo'.")]
    [InlineData("ref", null)]
    public void Validate_SupportsReferencedSchemas_IfDefinedInProvidedOpenApiDocument(
        string referenceId,
        string expectedExceptionMessage)
    {
        var openApiSchema = new OpenApiSchema
        {
            Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = referenceId }
        };
        var openApiDocument = new OpenApiDocument
        {
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, OpenApiSchema>
                {
                    ["ref"] = new OpenApiSchema { Type = JsonSchemaTypes.Number }
                }
            }
        };
        var instance = JToken.Parse("1");

        var exception = Record.Exception(() =>
        {
            var returnValue = Subject().Validate(
                openApiSchema,
                openApiDocument,
                instance,
                out IEnumerable<string> errorMessages);

            Assert.True(returnValue);
        });

        Assert.Equal(expectedExceptionMessage, exception?.Message);
    }

    private static JsonValidator Subject() => new();
}
