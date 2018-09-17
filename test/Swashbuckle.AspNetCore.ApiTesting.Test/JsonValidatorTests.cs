using System.Collections.Generic;
using Xunit;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class JsonValidatorTests
    {
        [Theory]
        [InlineData("null", "{}", false, "Path: . Instance is not of type 'null'")]
        [InlineData("null", "null", true, null)]
        [InlineData("boolean", "'foobar'", false, "Path: . Instance is not of type 'boolean'")]
        [InlineData("boolean", "true", true, null)]
        [InlineData("object", "'foobar'", false, "Path: . Instance is not of type 'object'")]
        [InlineData("object", "{}", true, null)]
        [InlineData("array", "'foobar'", false, "Path: . Instance is not of type 'array'")]
        [InlineData("array", "[]", true, null)]
        [InlineData("number", "'foobar'", false, "Path: . Instance is not of type 'number'")]
        [InlineData("number", "1", true, null)]
        [InlineData("string", "{}", false, "Path: . Instance is not of type 'string'")]
        [InlineData("string", "'foobar'", true, null)]
        public void Validate_ReturnsError_IfInstanceNotOfExpectedType(
            string schemaType,
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
            var openApiSchema = new OpenApiSchema { Type = "number", MultipleOf = schemaMultipleOf };
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
            var openApiSchema = new OpenApiSchema { Type = "number", Maximum = schemaMaximum };
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
                Type = "number",
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
            var openApiSchema = new OpenApiSchema { Type = "number", Minimum = schemaMinimum };
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
                Type = "number",
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
                Type = "string",
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
                Type = "string",
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
                Type = "string",
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

        [Theory]
        [InlineData("boolean", "[ true, 'foo' ]", false, "Path: [1]. Instance is not of type 'boolean'")]
        [InlineData("number", "[ 123, 'foo' ]", false, "Path: [1]. Instance is not of type 'number'")]
        [InlineData("boolean", "[ true, false ]", true, null)]
        public void Validate_ReturnsError_IfArrayItemDoesNotMatchItemsSchema(
            string itemsSchemaType,
            string instanceText,
            bool expectedReturnValue,
            string expectedErrorMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "array",
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
                Type = "array",
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
                Type = "array",
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
                Type = "array",
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
                Type = "object",
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
                Type = "object",
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
                Type = "object",
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

        [Theory]
        [InlineData("number", "{ \"id\": \"foo\" }", false, "Path: id. Instance is not of type 'number'")]
        [InlineData("string", "{ \"id\": 123 }", false, "Path: id. Instance is not of type 'string'")]
        [InlineData("number", "{ \"id\": 123 }", true, null)]
        public void Validate_ReturnsError_IfKnownPropertyDoesNotMatchPropertySchema(
            string propertySchemaType,
            string instanceText,
            bool expectedReturnValue,
            string expectedErrorMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
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

        [Theory]
        [InlineData("number", "{ \"id\": \"foo\" }", false, "Path: id. Instance is not of type 'number'")]
        [InlineData("string", "{ \"name\": 123 }", false, "Path: name. Instance is not of type 'string'")]
        [InlineData("number", "{ \"description\": 123 }", true, null)]
        public void Validate_ReturnsError_IfAdditionalPropertyDoesNotMatchAdditionalPropertiesSchema(
            string additionalPropertiesType,
            string instanceText,
            bool expectedReturnValue,
            string expectedErrorMessage)
        {
            var openApiSchema = new OpenApiSchema
            {
                Type = "object",
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
                Type = "object",
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
                AllOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p1" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p2" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p3" } }
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
                AnyOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p1" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p2" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p3" } }
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
                OneOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p1" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p2" } },
                    new OpenApiSchema { Type = "object", Required = new SortedSet<string> { "p3" } }
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
                        ["ref"] = new OpenApiSchema { Type = "number" }
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

        private JsonValidator Subject()
        {
            return new JsonValidator();
        }
    }
}
