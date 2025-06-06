using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.OpenApi.Models;

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

        var testCases = new TheoryData<string, bool, RangeAttribute, string, string>();

        foreach (var culture in invariantOrEnglishCultures)
        {
            foreach (var exclusive in isExclusive)
            {
                testCases.Add(culture, exclusive, new(1, 1234) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1d, 1234d) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1.23, 4.56) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                testCases.Add(culture, exclusive, new(typeof(float), "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                testCases.Add(culture, exclusive, new(typeof(float), "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive, ParseLimitsInInvariantCulture = true }, "1.23", "4.56");
            }
        }

        foreach (var culture in commaForDecimalCultures)
        {
            foreach (var exclusive in isExclusive)
            {
                testCases.Add(culture, exclusive, new(1, 1234) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1, 1234) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1d, 1234d) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1", "1234");
                testCases.Add(culture, exclusive, new(1.23, 4.56) { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                testCases.Add(culture, exclusive, new(typeof(float), "1,23", "4,56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive }, "1.23", "4.56");
                testCases.Add(culture, exclusive, new(typeof(float), "1.23", "4.56") { MaximumIsExclusive = exclusive, MinimumIsExclusive = exclusive, ParseLimitsInInvariantCulture = true }, "1.23", "4.56");
            }
        }

        // Numbers using numeric format, such as with thousands separators
        testCases.Add("en-GB", false, new(typeof(float), "-12,445.7", "12,445.7"), "-12445.7", "12445.7");
        testCases.Add("fr-FR", false, new(typeof(float), "-12 445,7", "12 445,7"), "-12445.7", "12445.7");
        testCases.Add("sv-SE", false, new(typeof(float), "-12 445,7", "12 445,7"), "-12445.7", "12445.7");

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
