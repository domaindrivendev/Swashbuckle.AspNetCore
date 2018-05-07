using Newtonsoft.Json;
using System;
using Xunit;

namespace Swashbuckle.AspNetCore.Cli.Test
{
    public class ProgramTests
    {
        private class ProgramSubclass : Program
        {
            // Subclassing in order to expose protected methods.
            public static T ExposedParseEnum<T>(string optionValue) where T : struct, IConvertible
            {
                return ParseEnum<T>(optionValue);
            }
        }

        [Theory]
        [InlineData(null, Formatting.None)]
        [InlineData("", Formatting.None)]
        [InlineData("Indented", Formatting.Indented)]
        [InlineData("indented", Formatting.Indented)]
        [InlineData("None", Formatting.None)]
        [InlineData("none", Formatting.None)]
        [InlineData("Can't parse me", Formatting.None)]
        public void ParseEnum_ParsesFormatOptionAccordingly_WhenOptionValueIsOrIsNotProvided(string optionValue, Formatting expectedResult)
        {
            var result = ProgramSubclass.ExposedParseEnum<Formatting>(optionValue);

            Assert.Equal(expectedResult, result);
        }
    }
}