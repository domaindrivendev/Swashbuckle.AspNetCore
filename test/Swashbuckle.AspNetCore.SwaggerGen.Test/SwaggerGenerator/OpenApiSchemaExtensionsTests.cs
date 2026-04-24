using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.OpenApi;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public static class OpenApiSchemaExtensionsTests
{
    public static TheoryData<string, bool, RangeAttribute, string, string> TestCases()
    {
        bool[] isExclusive = [false, true];

        string[] invariantOrEnglishCultures =
        [
            string.Empty,
            "en",
            "en-AU",
            "en-GB",
            "en-US",
        ];

        string[] commaForDecimalCultures =
        [
            "de-DE",
            "fr-FR",
            "sv-SE",
        ];

        Type[] fractionNumberTypes =
        [
            typeof(float),
            typeof(double),
            typeof(decimal),
        ];

        var testCases = new TheoryData<string, bool, RangeAttribute, string, string>();

        foreach (var culture in invariantOrEnglishCultures)
        {
            foreach (var exclusive in isExclusive)
            {
                testCases.Add(culture, exclusive, new(1, 1234) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1d, 1234d) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1.23, 4.56) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");

                foreach (var type in fractionNumberTypes)
                {
                    testCases.Add(culture, exclusive, new(type, "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                    testCases.Add(culture, exclusive, new(type, "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive, ParseLimitsInInvariantCulture = true }, "1.23", "4.56");
                }
            }
        }

        foreach (var culture in commaForDecimalCultures)
        {
            foreach (var exclusive in isExclusive)
            {
                testCases.Add(culture, exclusive, new(1, 1234) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1d, 1234d) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1.23, 4.56) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");

                foreach (var type in fractionNumberTypes)
                {
                    testCases.Add(culture, exclusive, new(type, "1,23", "4,56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                    testCases.Add(culture, exclusive, new(type, "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive, ParseLimitsInInvariantCulture = true }, "1.23", "4.56");
                }
            }
        }

        // Numbers using numeric format, such as with thousands separators
        testCases.Add("en-GB", false, new(typeof(float), "-12,445.7", "12,445.7"), "-12445.7", "12445.7");
        testCases.Add("fr-FR", false, new(typeof(float), "-12 445,7", "12 445,7"), "-12445.7", "12445.7");
        testCases.Add("sv-SE", false, new(typeof(float), "-12 445,7", "12 445,7"), "-12445.7", "12445.7");

        // Decimal value that would lose precision if parsed as a float or double
        foreach (var exclusive in isExclusive)
        {
            testCases.Add("en-US", exclusive, new(typeof(decimal), "12345678901234567890.123456789", "12345678901234567890.123456789") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "12345678901234567890.123456789", "12345678901234567890.123456789");
            testCases.Add("en-US", exclusive, new(typeof(decimal), "12345678901234567890.123456789", "12345678901234567890.123456789") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive, ParseLimitsInInvariantCulture = true }, "12345678901234567890.123456789", "12345678901234567890.123456789");
        }

        return testCases;
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public static void ApplyValidationAttributes_Handles_RangeAttribute_Correctly(
        string cultureName,
        bool isExclusive,
        RangeAttribute rangeAttribute,
        string expectedMinimum,
        string expectedMaximum)
    {
        // Arrange
        var schema = new OpenApiSchema();

        // Act
        using (CultureSwitcher.UseCulture(cultureName))
        {
            schema.ApplyValidationAttributes([rangeAttribute]);
        }

        // Assert
        if (isExclusive)
        {
            Assert.Equal(expectedMinimum, schema.ExclusiveMinimum);
            Assert.Equal(expectedMaximum, schema.ExclusiveMaximum);
            Assert.Null(schema.Minimum);
            Assert.Null(schema.Maximum);
        }
        else
        {
            Assert.Equal(expectedMinimum, schema.Minimum);
            Assert.Equal(expectedMaximum, schema.Maximum);
            Assert.Null(schema.ExclusiveMinimum);
            Assert.Null(schema.ExclusiveMaximum);
        }
    }

    [Fact]
    public static void ApplyValidationAttributes_Handles_Invalid_RangeAttribute_Values()
    {
        // Arrange
        var rangeAttribute = new RangeAttribute(typeof(int), "foo", "bar");
        var schema = new OpenApiSchema();

        // Act
        schema.ApplyValidationAttributes([rangeAttribute]);

        // Assert
        Assert.Null(schema.ExclusiveMinimum);
        Assert.Null(schema.ExclusiveMaximum);
        Assert.Null(schema.Minimum);
        Assert.Null(schema.Maximum);
    }

    [Fact]
    public static void ApplyValidationAttributes_Handles_DataTypeAttribute_CustomDataType_Correctly()
    {
        // Arrange
        string customDataType = "uuid";
        var dataTypeAttribute = new DataTypeAttribute(customDataType);
        var schema = new OpenApiSchema();

        // Act
        schema.ApplyValidationAttributes([dataTypeAttribute]);

        // Assert
        Assert.Equal(customDataType, schema.Format);
    }

    [Fact]
    public static void ApplyValidationAttributes_MinLength_On_Dictionary_Maps_To_MinProperties()
    {
        // Arrange — dictionary schema is represented as an Object with AdditionalProperties
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.String },
        };

        // Act
        schema.ApplyValidationAttributes([new MinLengthAttribute(1)]);

        // Assert
        Assert.Equal(1, schema.MinProperties);
        Assert.Null(schema.MinLength);
        Assert.Null(schema.MinItems);
    }

    [Fact]
    public static void ApplyValidationAttributes_MaxLength_On_Dictionary_Maps_To_MaxProperties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.String },
        };

        // Act
        schema.ApplyValidationAttributes([new MaxLengthAttribute(10)]);

        // Assert
        Assert.Equal(10, schema.MaxProperties);
        Assert.Null(schema.MaxLength);
        Assert.Null(schema.MaxItems);
    }

    [Fact]
    public static void ApplyValidationAttributes_Length_On_Dictionary_Maps_To_Min_And_MaxProperties()
    {
        // Arrange
        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.String },
        };

        // Act
        schema.ApplyValidationAttributes([new LengthAttribute(2, 5)]);

        // Assert
        Assert.Equal(2, schema.MinProperties);
        Assert.Equal(5, schema.MaxProperties);
        Assert.Null(schema.MinLength);
        Assert.Null(schema.MaxLength);
        Assert.Null(schema.MinItems);
        Assert.Null(schema.MaxItems);
    }

    [Fact]
    public static void ApplyValidationAttributes_MinLength_On_String_Still_Maps_To_MinLength()
    {
        // Arrange — regression guard for the existing string path
        var schema = new OpenApiSchema { Type = JsonSchemaType.String };

        // Act
        schema.ApplyValidationAttributes([new MinLengthAttribute(3)]);

        // Assert
        Assert.Equal(3, schema.MinLength);
        Assert.Null(schema.MinProperties);
        Assert.Null(schema.MinItems);
    }

    [Fact]
    public static void ApplyValidationAttributes_MinLength_On_Array_Still_Maps_To_MinItems()
    {
        // Arrange — regression guard for the existing array path
        var schema = new OpenApiSchema { Type = JsonSchemaType.Array };

        // Act
        schema.ApplyValidationAttributes([new MinLengthAttribute(3)]);

        // Assert
        Assert.Equal(3, schema.MinItems);
        Assert.Null(schema.MinProperties);
        Assert.Null(schema.MinLength);
    }

    private sealed class CultureSwitcher : IDisposable
    {
        private readonly CultureInfo _previous;

        private CultureSwitcher(string name)
        {
            _previous = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name);
        }

        public static CultureSwitcher UseCulture(string name) => new(name);

        public void Dispose()
        {
            if (_previous is not null)
            {
                CultureInfo.CurrentCulture = _previous;
            }
        }
    }
}
