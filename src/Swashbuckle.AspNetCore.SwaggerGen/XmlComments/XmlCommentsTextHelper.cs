using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public static class XmlCommentsTextHelper
    {
        private static Regex RefTagPattern = new Regex(@"<(see|paramref) (name|cref)=""([TPF]{1}:)?(?<display>.+?)"" ?/>");
        private static Regex CodeTagPattern = new Regex(@"<c>(?<display>.+?)</c>");
        private static Regex ParaTagPattern = new Regex(@"<para>(?<display>.+?)</para>", RegexOptions.Singleline);

        public static string Humanize(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            //Call DecodeXml at last to avoid entities like &lt and &gt to break valid xml          

            return text
                .NormalizeIndentation()
                .HumanizeRefTags()
                .HumanizeCodeTags()
                .HumanizeParaTags()
                .DecodeXml();
        }

        private static string NormalizeIndentation(this string text)
        {
            string[] lines = text.Split('\n');
            string padding = GetCommonLeadingWhitespace(lines);

            int padLen = padding == null ? 0 : padding.Length;

            // remove leading padding from each line
            for (int i = 0, l = lines.Length; i < l; ++i)
            {
                string line = lines[i].TrimEnd('\r'); // remove trailing '\r'

                if (padLen != 0 && line.Length >= padLen && line.Substring(0, padLen) == padding)
                    line = line.Substring(padLen);

                lines[i] = line;
            }

            // remove leading empty lines, but not all leading padding
            // remove all trailing whitespace, regardless
            return string.Join("\r\n", lines.SkipWhile(x => string.IsNullOrWhiteSpace(x))).TrimEnd();
        }

        private static string GetCommonLeadingWhitespace(string[] lines)
        {
            if (null == lines)
                throw new ArgumentException("lines");

            if (lines.Length == 0)
                return null;

            string[] nonEmptyLines = lines
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            if (nonEmptyLines.Length < 1)
                return null;

            int padLen = 0;

            // use the first line as a seed, and see what is shared over all nonEmptyLines
            string seed = nonEmptyLines[0];
            for (int i = 0, l = seed.Length; i < l; ++i)
            {
                if (!char.IsWhiteSpace(seed, i))
                    break;

                if (nonEmptyLines.Any(line => line[i] != seed[i]))
                    break;

                ++padLen;
            }

            if (padLen > 0)
                return seed.Substring(0, padLen);

            return null;
        }

        private static string HumanizeRefTags(this string text)
        {
            return RefTagPattern.Replace(text, (match) => match.Groups["display"].Value);
        }

        private static string HumanizeCodeTags(this string text)
        {
            return CodeTagPattern.Replace(text, (match) => "{" + match.Groups["display"].Value + "}");
        }

        private static string HumanizeParaTags(this string text)
        {
            return ParaTagPattern.Replace(text, (match) => "<br>" + match.Groups["display"].Value);
        }

        private static string DecodeXml(this string text)
        {
            return WebUtility.HtmlDecode(text);
        }

    }
}