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
        var minimum = decimal.Parse(expectedMinimum, CultureInfo.InvariantCulture);
        var maximum = decimal.Parse(expectedMaximum, CultureInfo.InvariantCulture);

        var schema = new OpenApiSchema();

        // Act
        using (CultureSwitcher.UseCulture(cultureName))
        {
            schema.ApplyValidationAttributes([rangeAttribute]);
        }

        // Assert
        Assert.Equal(isExclusive ? true : null, schema.ExclusiveMinimum);
        Assert.Equal(isExclusive ? true : null, schema.ExclusiveMaximum);
        Assert.Equal(minimum, schema.Minimum);
        Assert.Equal(maximum, schema.Maximum);
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
