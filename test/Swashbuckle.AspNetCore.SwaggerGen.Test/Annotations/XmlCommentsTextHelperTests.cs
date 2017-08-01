using Xunit;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// NOTE: Whitespace in these tests is significant and uses a combination of {tabs} and {spaces}
    /// You should toggle "View White Space" to "on".
    public class XmlCommentsTextHelperTests
    {
        [Fact]
        public void Humanize_TrimsWhiteSpaceFromStartAndEnd()
        {
            var input = @"
 A line of text
            ";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal("A line of text", output, false, true);
        }

        [Fact]
        public void Humanize_TrimsCommonSpaceIndentations()
        {
            var input = @"
                ## Heading 1

                  * list item 1

                ## Heading 2

                	POST /api/test

                	{
                	  ""prop1"": {
                	    ""name"": ""value""
                	  }
                	}
            ";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(
@"## Heading 1

  * list item 1

## Heading 2

	POST /api/test

	{
	  ""prop1"": {
	    ""name"": ""value""
	  }
	}",
                output,
                false,
                true
            );
        }

        [Fact]
        public void Humanize_TrimsCommonTabIndentations()
        {
            // Common indentation seen in visual studio: {tab}
            var input = @"
				## Heading 1

				A line of text
			";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(
@"## Heading 1

A line of text",
                output,
                false,
                true
            );
        }

        [Fact]
        public void Humanize_TrimsCommonSpaceTabIndentations()
        {
            // Common indentation seen in visual studio: {space}{tab}
            var input = @"
 				## Heading 1

 				A line of text
 			";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(
@"## Heading 1

A line of text",
                output,
                false,
                true
            );
        }

        [Fact]
        public void Humanize_DoesNotTrimInconsistentIndentations()
        {
            var input = @"
                Space Indentation Line 1
                Space Indentation Line 2
            	Misplaced Tab Indentation
                Space Indentation Line 4
            ";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(
@"    Space Indentation Line 1
    Space Indentation Line 2
	Misplaced Tab Indentation
    Space Indentation Line 4",
            output, false, true);
        }

        [Theory]
        [InlineData(@"Returns a <see cref=""T:Product""/>", "Returns a Product")]
        [InlineData(@"<paramref name=""param1"" /> does something", "param1 does something")]
        [InlineData(@"<c>DoWork</c> is a method in <c>TestClass</c>.", "{DoWork} is a method in {TestClass}.")]
        public void Humanize_HumanizesInlineTags(
            string input,
            string expectedOutput)
        {
            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(expectedOutput, output, false, true);
        }
    }
}
