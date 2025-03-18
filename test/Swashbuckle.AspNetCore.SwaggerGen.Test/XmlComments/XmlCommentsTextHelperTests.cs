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
        [InlineData("<c>DoWork</c> is a method in <c>TestClass</c>.", "`DoWork` is a method in `TestClass`.")]
        [InlineData("<code>DoWork</code> is a method in <code>\nTestClass\n</code>.", "```DoWork``` is a method in ```\nTestClass\n```.")]
        [InlineData("<para>This is a paragraph</para>.", "\r\nThis is a paragraph.")]
        [InlineData("<para>   This is a paragraph   </para>.", "\r\nThis is a paragraph.")]
        [InlineData("GET /Todo?iscomplete=true&amp;owner=mike", "GET /Todo?iscomplete=true&owner=mike")]
        [InlineData(@"Returns a <see langword=""null""/> item.", "Returns a null item.")]
        [InlineData(@"<see href=""https://www.iso.org/iso-4217-currency-codes.html"">ISO currency code</see>", "[ISO currency code](https://www.iso.org/iso-4217-currency-codes.html)")]
        [InlineData("First line.<br />Second line.<br/>Third line.<br>Fourth line.", "First line.\r\nSecond line.\r\nThird line.\r\nFourth line.")]
        [InlineData("<para> one </para><para> two </para>","\r\none\r\ntwo")]
        public void Humanize_HumanizesInlineTags(
            string input,
            string expectedOutput)
        {
            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal(expectedOutput, output, false, true);
        }

        [Theory]
        [InlineData("\r\n")]
        [InlineData("\n")]
        [InlineData(null)]
        public void Humanize_MultilineBrTag_SpecificEol(string xmlCommentEndOfLine)
        {
            const string input = @"
            This is a paragraph.
            <br>
            A parameter after br tag.";

            var output = XmlCommentsTextHelper.Humanize(input, xmlCommentEndOfLine);

            var expected = string.Join(XmlCommentsTextHelper.EndOfLine(xmlCommentEndOfLine),
            [
                "This is a paragraph.",
                "",
                "",
                "A parameter after br tag."
            ]);
            Assert.Equal(expected, output, false, ignoreLineEndingDifferences: false);
        }

        [Fact]
        public void Humanize_ParaMultiLineTags()
        {
            const string input = @"
            <para>
             This is a paragraph.
             MultiLined.
            </para>
            <para>         This is a paragraph     </para>.";

            var output = XmlCommentsTextHelper.Humanize(input);

            Assert.Equal("\r\nThis is a paragraph.\r\n MultiLined.\r\n\r\nThis is a paragraph.", output, false, true);
        }

        [Fact]
        public void Humanize_CodeMultiLineTag()
        {
            const string input = @"
            <code>
               {
                ""Prop1"":1,
                ""Prop2"":[]
               }
            </code>";

            var output = XmlCommentsTextHelper.Humanize(input);

            var expected = string.Join(XmlCommentsTextHelper.EndOfLine(null),
            [
                "```",
                "{",
                " \"Prop1\":1,",
                " \"Prop2\":[]",
                "}",
                "```"
            ]);
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Humanize_CodeMultiLineTag_OnSameLine()
        {
            const string input = @"
            <code>{
                ""Prop1"":1,
                ""Prop2"":[]
               }
            </code>";

            var output = XmlCommentsTextHelper.Humanize(input);

            var expected = string.Join(XmlCommentsTextHelper.EndOfLine(null),
            [
                "```",
                "{",
                "    \"Prop1\":1,",
                "    \"Prop2\":[]",
                "   }",
                "```"
            ]);
            Assert.Equal(expected, output);
        }

        [Fact]
        public void Humanize_CodeInsideParaTag()
        {
            var input = string.Join(XmlCommentsTextHelper.EndOfLine(null),
            [
                "<para>Creates a new Answer</para>",
                "<para><code><![CDATA[",
                "POST /api/answers",
                "{",
                """  "name": "OnlyYes",""",
                """  "label": "Yes",""",
                """  "answers": [""",
                "                 {",
                """                     "answer": "yes""",
                "                 }",
                "             ]",
                "}",
                "]]></code></para>",
            ]);

            var output = XmlCommentsTextHelper.Humanize(input);

            var expected = string.Join(XmlCommentsTextHelper.EndOfLine(null),
            [
                "",
                "Creates a new Answer",
                "",
                "```",
                "<![CDATA[",
                "POST /api/answers",
                "{",
                """  "name": "OnlyYes",""",
                """  "label": "Yes",""",
                """  "answers": [""",
                "                 {",
                """                     "answer": "yes""",
                "                 }",
                "             ]",
                "}",
                "]]>",
                "```"
            ]);
            Assert.Equal(expected, output);
        }
    }
}
