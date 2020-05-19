using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal static class XmlTransform
    {
        /// <summary>Converts the given XML documentation to Markdown.</summary>
        /// <param name="input">InnerXML of an element.</param>
        /// <returns>The Markdown</returns>
        public static string ToMarkdown(string input)
        {
            // ToMarkdown -> ToMarkdownNode  <-> ToMarkdownNodeCollection
            // ^- top level  ^- inside of XML ^- mutual recursion
            var dedented = DedentElement(input.Replace("\r", string.Empty));
            // pseudo xml container is required to call `XElement.Parse` since input is `InnerXml`
            var element = XElement.Parse("<xml>" + dedented + "</xml>",
                LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);

            var collected = new List<MarkdownNode>();
            var lastIndent = 0;
            foreach (var node in element.Nodes())
            {
                if (node is XElement e && ToMarkdownNode(e) is var markdownNode && markdownNode != null)
                {
                    collected.Add(new TopLevelWrappedNode(lastIndent, markdownNode));
                }
                else
                {
                    var text = node.ToString().Replace("\r", string.Empty);
                    var lastNewline = text.LastIndexOf('\n');
                    if (lastNewline == -1)
                    {
                        if (collected.Count == 0)
                        {
                            lastIndent = text.Length;
                        }
                        else
                        {
                            lastIndent = 0;
                        }
                    }
                    else
                    {
                        lastIndent = text.Length - lastNewline - 1;
                    }

                    collected.Add(ConvertRawTextNode(text));
                }
            }

            var delimited = CreateTopLevelDelimitedNodeCollection(collected);

            return delimited.Render();
        }

        private static NodeCollection<MarkdownNode> ToMarkdownNodeCollection(IEnumerable<XNode> nodes)
        {
            var collected = new List<MarkdownNode>();
            foreach (var node in nodes)
            {
                if (node is XElement e && ToMarkdownNode(e) is var markdownNode && markdownNode != null)
                {
                    collected.Add(markdownNode);
                }
                else if (node is XText textNode)
                {
                    collected.Add(ConvertTextNode(textNode.Value));
                }

                // Skip XComment, XDocumentType, XProcessingInstruction, XDocument
            }

            return CreateDelimitedNodeCollection(collected);
        }

        private static MarkdownNode ConvertTextNode(string text)
        {
            var lines = text.Split('\n');
            // Assert(lines.Length > 0)
            if (lines.All(string.IsNullOrWhiteSpace))
            {
                switch (lines.Length)
                {
                    case 1:
                        return SpaceNode.Default;
                    default:
                        return NewlineNode.Default;
                }
            }

            var skip = 0;
            var take = lines.Length;

            DelimiterNode startedWith = SpaceNode.None;
            if (string.IsNullOrWhiteSpace(lines.First()))
            {
                startedWith = NewlineNode.Default;
                skip++;
                take--;
            }
            else if (lines.First().StartsWith(" ") || lines.First().StartsWith("\t"))
            {
                startedWith = SpaceNode.Default;
            }

            DelimiterNode endedWith = SpaceNode.None;
            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                endedWith = NewlineNode.Default;
                take--;
            }
            else if (lines.Last().EndsWith(" ") || lines.Last().EndsWith("\t"))
            {
                endedWith = SpaceNode.Default;
            }

            var collected = lines.Skip(skip)
                .Take(take)
                .Select(line => line.Trim())
                .Where(x => x.Length > 0);
            var joined = string.Join("\n", collected);
            return new TextNode(joined, startedWith, endedWith);
        }

        /// <summary>
        /// Create <see cref="NodeCollection{T}"/> with delimiter heuristics.
        /// </summary>
        private static NodeCollection<MarkdownNode> CreateDelimitedNodeCollection(List<MarkdownNode> collected)
        {
            var result = new NodeCollection<MarkdownNode>();
            if (!collected.Any())
            {
                return result;
            }

            var start = 0;
            // Trim the first empty line
            if (collected[0] is TextNode first && string.IsNullOrWhiteSpace(first.Value) ||
                collected[0] is DelimiterNode)
            {
                start = 1;
            }

            var end = collected.Count - 1;
            // Trim the last empty line
            if (collected[end] is TextNode last && string.IsNullOrWhiteSpace(last.Value) ||
                collected[end] is DelimiterNode)
            {
                end -= 1;
            }

            for (var i = start; i < end; i++)
            {
                var left = collected[i];
                var right = collected[i + 1];

                InsertHeuristicDelimiter(result, left, right, null, NewlineNode.Default);
            }

            if (end >= 0)
            {
                result.Add(collected[end]);
            }

            return result;
        }

        private static void InsertHeuristicDelimiter(
            NodeCollection<MarkdownNode> result,
            MarkdownNode left,
            MarkdownNode right,
            MarkdownNode lookahead,
            NewlineNode defaultNewlineDelimiter)
        {
            if (result.LastOrDefault() is NewlineNode && left is DelimiterNode)
            {
                // Existing delimiter was preempted by newline.
                return;
            }

            if (left.IsInline && right.IsInline)
            {
                result.Add(left);
                if (left.GetType() == right.GetType() && left.SpaceBetweenInlines)
                {
                    result.Add(SpaceNode.Default);
                }

                if (right is NewlineNode rightNewline && lookahead != null && lookahead.IsInline)
                {
                    // preempt next newline
                    result.Add(NewlineNode.GetStrongest(rightNewline, NewlineNode.Hard));
                }
                else if (right is TextNode rightText)
                {
                    if (rightText.StartedWith is NewlineNode)
                        result.Add(NewlineNode.Default);
                    else
                        result.Add(rightText.StartedWith);
                }
                else if (left is TextNode leftText)
                {
                    if (leftText.EndedWith is NewlineNode)
                        result.Add(NewlineNode.Default);
                    else
                        result.Add(leftText.EndedWith);
                }
            }
            else if (left.IsInline /* && right.IsBlock */)
            {
                // Ensure new lines between inline-block
                if (left is NewlineNode newline)
                {
                    result.Add(NewlineNode.GetStrongest(newline, right.DelimiterBeforeBlock));
                }
                else if (left is SpaceNode)
                {
                    result.Add(NewlineNode.GetStrongest(defaultNewlineDelimiter, right.DelimiterBeforeBlock));
                }
                else
                {
                    result.Add(left);
                    result.Add(right.DelimiterBeforeBlock);
                }
            }
            else /* left.IsBlock */
            {
                result.Add(left);
                // Ensure new lines after blocks.
                // This will preempt the following delimiter.
                var strongest = NewlineNode.GetStrongest(defaultNewlineDelimiter, left.DelimiterAfterBlock);
                if (right is NewlineNode newline)
                {
                    strongest = NewlineNode.GetStrongest(strongest, newline);
                }

                result.Add(strongest);
            }

            // Since IsBlock != IsInline, all cases are covered
        }

        private static MarkdownNode ConvertRawTextNode(string text)
        {
            var lines = text.Split('\n');
            // Assert(lines.Length > 0)
            if (lines.All(string.IsNullOrWhiteSpace))
            {
                switch (lines.Length)
                {
                    case 1:
                        return SpaceNode.Default;
                    default:
                        return new NewlineNode(lines.Length - 1);
                }
            }

            var skip = 0;
            var take = lines.Length;

            DelimiterNode startedWith = SpaceNode.None;
            if (string.IsNullOrWhiteSpace(lines.First()))
            {
                startedWith = NewlineNode.Default;
                skip++;
                take--;
            }
            else if (lines.First().StartsWith(" ") || lines.First().StartsWith("\t"))
            {
                startedWith = SpaceNode.Default;
            }

            DelimiterNode endedWith = SpaceNode.None;
            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                endedWith = NewlineNode.Default;
                take--;
            }
            else if (lines.Last().EndsWith(" ") || lines.Last().EndsWith("\t"))
            {
                endedWith = new SpaceNode(Regex.Match(lines.Last(), "[ \t]*$").Length);
            }

            var collected = lines.Skip(skip).Take(take).Select(line => line.TrimEnd()).ToList();
            if (collected.Count > 0 && !Equals(startedWith, NewlineNode.Default))
            {
                collected[0] = collected[0].TrimStart();
            }

            var joined = string.Join("\n", collected);
            return new TextNode(joined, startedWith, endedWith);
        }

        /// <summary>
        /// Create Top level <see cref="NodeCollection{T}"/> with delimiter heuristics.
        /// </summary>
        private static NodeCollection<MarkdownNode> CreateTopLevelDelimitedNodeCollection(List<MarkdownNode> collected)
        {
            var result = new NodeCollection<MarkdownNode>();
            var end = Math.Max(collected.Count - 1, 0);
            for (var i = 0; i < end; i++)
            {
                var left = collected[i];
                var right = collected[i + 1];

                if (!(left is TopLevelWrappedNode) && !(right is TopLevelWrappedNode))
                {
                    throw new InvalidOperationException("Unreachable");
                }

                if (left is TextNode leftText && right is TopLevelWrappedNode rightWrapped)
                {
                    if (right.IsBlock)
                    {
                        result.Add(left);
                        if (leftText.EndedWith is NewlineNode)
                            result.Add(right.DelimiterBeforeBlock);
                        else
                            result.Add(leftText.EndedWith);
                        if (rightWrapped.Indent != 0 && leftText.EndedWith is NewlineNode)
                        {
                            result.Add(new SpaceNode(rightWrapped.Indent));
                        }
                    }
                    else if (leftText.EndedWith is NewlineNode)
                    {
                        result.Add(left);
                        result.Add(NewlineNode.Default);

                        if (rightWrapped.Indent != 0)
                        {
                            result.Add(new SpaceNode(rightWrapped.Indent));
                        }
                    }
                    else
                    {
                        result.Add(left);
                        result.Add(leftText.EndedWith.Normalize());
                    }
                }
                else if (left is TopLevelWrappedNode leftWrapped && right is TextNode rightText)
                {
                    if (left.IsBlock)
                    {
                        result.Add(left);
                        result.Add(left.DelimiterAfterBlock);
                        if (leftWrapped.Indent != 0 && !(rightText.StartedWith is NewlineNode))
                        {
                            result.Add(new SpaceNode(leftWrapped.Indent));
                        }
                    }
                    else
                    {
                        result.Add(left);
                        result.Add(rightText.StartedWith.Normalize());
                    }
                }
                else
                {
                    var lookahead = i + 2 > end ? null : collected[i + 2];
                    if (right is NewlineNode newline)
                    {
                        InsertHeuristicDelimiter(result, left, right, lookahead, newline);
                    }
                    else
                    {
                        InsertHeuristicDelimiter(result, left, right, lookahead, NewlineNode.Default);
                    }

                    if (result.LastOrDefault() is NewlineNode && right is TopLevelWrappedNode wrapped && wrapped.Indent != 0)
                    {
                        result.Add(new SpaceNode(wrapped.Indent));
                    }
                }
            }

            if (collected.LastOrDefault() != null)
            {
                result.Add(collected.Last());
            }

            return result;
        }

        /// They are defined in <a href="https://spec.commonmark.org/0.29/#html-blocks">CommonMark spec</a>
        private static readonly HashSet<string> htmlBlocks =
            new HashSet<string>
            {
                "address", "article", "aside", "base", "basefont", "blockquote", "body", "caption", "center", "col",
                "colgroup", "dd", "details", "dialog", "dir", "div", "dl", "dt", "fieldset", "figcaption", "figure",
                "footer", "form", "frame", "frameset", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hr",
                "html", "iframe", "legend", "li", "link", "main", "menu", "menuitem", "nav", "noframes", "ol",
                "optgroup", "option", "p", "param", "section", "source", "summary", "table", "tbody", "td", "tfoot",
                "th", "thead", "title", "tr", "track", "ul"
            };

        private static MarkdownNode ToMarkdownNode(XElement element)
        {
            if (element.Name == "see")
            {
                var langword = element.Attribute("langword");
                if (langword != null)
                {
                    return new CodeNode(langword.Value);
                }

                var cref = element.Attribute("cref");
                if (cref != null)
                {
                    var match = Regex.Match(cref.Value, "^([TRF]{1}:)?(?<display>.+)$");
                    var crefValue = match.Groups["display"];
                    return new CodeNode(crefValue.Value);
                }

                var href = element.Attribute("href")?.Value ?? string.Empty;

                return new AutoLinkNode(href);
            }

            if (element.Name == "a")
            {
                // assume all inline
                var text = ToMarkdownNodeCollection(element.Nodes());
                var hrefAttribute = element.Attribute("href");
                var href = hrefAttribute != null ? hrefAttribute.Value : string.Empty;
                return new LinkNode(text, href) { Title = element.Attribute("title")?.Value };
            }

            if (element.Name == "c")
            {
                return new CodeNode(DedentInlineCodeElement(element));
            }

            if (element.Name == "code")
            {
                return new CodeBlockNode(DedentElement(element))
                {
                    Info = element.Attribute("lang")?.Value // Non standard, but useful extension
                };
            }

            if (element.Name == "paramref" || element.Name == "typeparamref")
            {
                var name = element.Attribute("name")?.Value ?? string.Empty;
                return new CodeNode(name);
            }

            if (element.Name == "para")
            {
                return new ParagraphNode(ToMarkdownNodeCollection(element.Nodes()));
            }

            if (element.Name == "list")
            {
                var type = element.Attribute("type")?.Value ?? "number";
                var items = element.Nodes().OfType<XElement>().Where(x => x.Name == "item");
                if (type == "bullet" || type == "number")
                {
                    var node = type == "bullet" ? (ListNode) new BulletListNode() : new NumberedListNode();
                    foreach (var item in items)
                    {
                        // JetBrains Rider prefers <description> element.
                        var description = item.Nodes().OfType<XElement>().FirstOrDefault(x => x.Name == "description");
                        node.Add(ToMarkdownNodeCollection(description != null ? description.Nodes() : item.Nodes()));
                    }

                    return node;
                }

                if (type == "table")
                {
                    var node = new TableNode();

                    var listHeader = element.Nodes().OfType<XElement>().FirstOrDefault(x => x.Name == "listheader");
                    if (listHeader != null)
                    {
                        foreach (var headerElement in listHeader.Nodes().OfType<XElement>())
                        {
                            node.AddHeader(headerElement.Name.LocalName, DedentElement(headerElement));
                        }
                    }

                    foreach (var item in items)
                    {
                        var row = node.CreateRow();
                        foreach (var itemElement in item.Nodes().OfType<XElement>())
                        {
                            row[itemElement.Name.LocalName] = ToMarkdownNodeCollection(itemElement.Nodes());
                        }
                    }

                    return node;
                }
            }

            if (htmlBlocks.Contains(element.Name.LocalName))
            {
                return new HtmlBlockNode(DedentHtmlElement(element));
            }

            return new InlineHtmlNode(DedentHtmlElement(element));
        }

        private static string DedentElement(string value)
        {
            // e.g.
            // <code>
            //           some
            //        irregular
            //            indentation
            //    </code>
            //        ^- adjust to here
            var lines = SplitLine(value);
            var indent = GetIndentation(lines);
            return Dedent(lines, line => TrimWhitespacesAtMost(indent, line));
        }

        private static string DedentElement(XElement element)
        {
            return DedentElement(element.Value);
        }

        private static string DedentInlineCodeElement(XElement element)
        {
            // e.g.
            // <c>some
            //
            //        irregular
            //
            //
            //            indentation
            //    </c>
            // =>
            // `some
            //        irregular
            //            indentation`
            var lines = SplitLine(element.Value);
            return Dedent(
                lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList(),
                line => line);
        }

        private static string DedentHtmlElement(XElement element)
        {
            // e.g.
            // <h1>
            //           some
            //            header
            //       </h1>
            //       ^- adjust to here
            var lines = SplitLine(element.ToString());
            var indent = GetIndentation(lines);

            var builder = new StringBuilder();
            if (lines.Count > 0)
            {
                builder.Append(lines[0]); // Append without dedent since XNode.ToString() doesn't indent first line.
            }

            foreach (var line in lines.Skip(1))
            {
                builder.Append('\n');
                builder.Append(line.Substring(indent));
            }

            return builder.ToString();
        }

        private static string Dedent(IList<string> lines, Func<string, string> dedent)
        {
            var builder = new StringBuilder();
            if (lines.Count > 0)
            {
                builder.Append(dedent(lines[0]));
            }

            foreach (var line in lines.Skip(1))
            {
                builder.Append('\n');
                builder.Append(dedent(line));
            }

            return builder.ToString();
        }

        private static List<string> SplitLine(string value)
        {
            var lines = value.Split('\n');
            var skip = 0;
            var take = lines.Length;

            if (string.IsNullOrWhiteSpace(lines.First()))
            {
                skip++;
                take--;
            }

            if (string.IsNullOrWhiteSpace(lines.Last()))
            {
                take--;
            }

            return lines.Skip(skip).Take(take).ToList();
        }

        private static int GetIndentation(IEnumerable<string> lines)
        {
            var enumerator = lines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .GetEnumerator();
            try
            {
                if (!enumerator.MoveNext())
                {
                    return 0;
                }

                var min = int.MaxValue;
                do
                {
                    min = Math.Min(min, Regex.Match(enumerator.Current, "^\\s*").Length);
                } while (enumerator.MoveNext());

                return min;
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private static string TrimWhitespacesAtMost(int count, string text)
        {
            var nonWsIndex = 0;
            while (nonWsIndex < text.Length && char.IsWhiteSpace(text[nonWsIndex]) && nonWsIndex < count)
            {
                nonWsIndex++;
            }

            return text.Substring(nonWsIndex);
        }
    }
}