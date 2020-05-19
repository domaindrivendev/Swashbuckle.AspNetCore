using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    internal abstract class MarkdownNode
    {
        public string Render(int indent = 0)
        {
            var context = new RenderContext(new StringBuilder(), indent, indent);
            RenderInternal(context);
            return context.ToString();
        }

        internal virtual bool IsBlock => false;
        internal virtual bool IsInline => !IsBlock;

        /// <summary>
        /// It is used when <c>IsBlock</c>
        /// </summary>
        internal virtual NewlineNode DelimiterBeforeBlock => IsInline ? throw new InvalidOperationException() : NewlineNode.Default;

        /// <summary>
        /// It is used when <c>IsBlock</c>
        /// </summary>
        internal virtual NewlineNode DelimiterAfterBlock => IsInline ? throw new InvalidOperationException() : NewlineNode.Default;

        internal virtual bool SpaceBetweenInlines => IsBlock ? throw new InvalidOperationException() : false;

        /// <summary>
        /// General rules
        /// <list type="number">
        /// <item><description>Don't put a newline character at the end unless it is necessary.</description></item>
        /// <item>
        /// <description>
        ///   Call <c>context.IndentForFirstLine()</c> or its equivalent inside of this function for the first line
        ///   if this is <c>IsBlock</c> element.
        /// </description>
        /// </item>
        /// </list>
        /// </summary>
        internal abstract void RenderInternal(RenderContext context);

        protected static void RenderIndented(RenderContext context, string value)
        {
            var lines = value.Split('\n');
            if (lines.Length == 0)
            {
                return;
            }

            if (!string.IsNullOrEmpty(lines[0]))
            {
                context.IndentForFirstLine();
                context.Append(lines[0]);
            }
            else
            {
                // Can omit context.IndentForFirstLine();
                // They are dedented later anyway.
            }

            foreach (var line in lines.Skip(1))
            {
                context.AppendLine();
                if (!string.IsNullOrEmpty(line))
                    context.IndentForRest();
                context.Append(line);
            }
        }

        public override string ToString()
        {
            return Render();
        }
    }

    internal sealed class RenderContext
    {
        internal RenderContext(StringBuilder builder, int indentForFirstLine, int indentForRest)
        {
            _builder = builder;
            _indentForFirstLine = indentForFirstLine;
            _indentForRest = indentForRest;
        }

        private readonly StringBuilder _builder;
        private readonly int _indentForFirstLine;
        private readonly int _indentForRest;

        public RenderContext WithNoIndentation(int amount = 0)
        {
            return new RenderContext(_builder, 0, _indentForRest + amount);
        }

        public RenderContext WithIndentationForRest(int amount = 0)
        {
            return new RenderContext(_builder, _indentForRest + amount, _indentForRest + amount);
        }

        public RenderContext WithIndentationForTopLevel(int indentForRest)
        {
            return new RenderContext(_builder, 0, indentForRest);
        }

        public void IndentForFirstLine()
        {
            Indent(_indentForFirstLine);
        }

        public void IndentForRest()
        {
            Indent(_indentForRest);
        }

        private void Indent(int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                _builder.Append(' ');
            }
        }

        public void Append(string value = null)
        {
            _builder.Append(value);
        }

        public void Append(char value)
        {
            _builder.Append(value);
        }

        public void AppendLine(string value = null)
        {
            _builder.Append(value);
            _builder.Append('\n');
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }

    internal class TopLevelWrappedNode : MarkdownNode
    {
        public TopLevelWrappedNode(int indent, MarkdownNode inner)
        {
            Inner = inner;
            Indent = indent;
        }

        public int Indent { get; }
        private MarkdownNode Inner { get; }

        internal override bool IsBlock => Inner.IsBlock;
        internal override bool IsInline => Inner.IsInline;
        internal override NewlineNode DelimiterBeforeBlock => Inner.DelimiterBeforeBlock;
        internal override NewlineNode DelimiterAfterBlock => Inner.DelimiterAfterBlock;
        internal override bool SpaceBetweenInlines => Inner.SpaceBetweenInlines;

        internal override void RenderInternal(RenderContext context)
        {
            Inner.RenderInternal(context.WithIndentationForTopLevel(Indent));
        }
    }

    internal class TextNode : MarkdownNode
    {
        public TextNode(string value, DelimiterNode startedWith, DelimiterNode endedWith)
        {
            Value = value;
            StartedWith = startedWith;
            EndedWith = endedWith;
        }

        public string Value { get; }
        public DelimiterNode StartedWith { get; }
        public DelimiterNode EndedWith { get; }

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context.WithNoIndentation(), Value);
        }

        public override string ToString()
        {
            return $"{StartedWith}{Value}{EndedWith}";
        }
    }

    internal sealed class ParagraphNode : MarkdownNode
    {
        public ParagraphNode(MarkdownNode value)
        {
            _value = value;
        }

        private readonly MarkdownNode _value;

        internal override bool IsBlock => true;
        internal override NewlineNode DelimiterBeforeBlock => NewlineNode.Hard;
        internal override NewlineNode DelimiterAfterBlock => NewlineNode.Hard;

        internal override void RenderInternal(RenderContext context)
        {
            _value.RenderInternal(context);
        }
    }

    internal sealed class InlineHtmlNode : MarkdownNode
    {
        public InlineHtmlNode(string value)
        {
            _value = value;
        }

        private readonly string _value;

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context.WithNoIndentation(), _value);
        }
    }

    internal sealed class HtmlBlockNode : MarkdownNode
    {
        public HtmlBlockNode(string value)
        {
            _value = value;
        }

        private readonly string _value;

        internal override bool IsBlock => true;

        internal override void RenderInternal(RenderContext context)
        {
            RenderIndented(context, _value);
        }
    }

    internal sealed class AutoLinkNode : MarkdownNode
    {
        public AutoLinkNode(string url)
        {
            _url = url;
        }

        private readonly string _url;

        internal override void RenderInternal(RenderContext context)
        {
            context.Append('<');
            context.Append(_url);
            context.Append('>');
        }
    }

    internal sealed class LinkNode : MarkdownNode
    {
        public LinkNode(MarkdownNode text, string url)
        {
            _text = text;
            _url = url;
        }

        private readonly MarkdownNode _text;
        private readonly string _url;

        public string Title { get; set; }

        internal override void RenderInternal(RenderContext context)
        {
            context.Append('[');
            RenderText(context, _text.Render());
            context.Append(']');

            context.Append('(');
            RenderUrl(context, _url);
            if (Title != null)
            {
                context.Append(' ');
                RenderTitle(context, Title);
            }

            context.Append(')');
        }

        private static void RenderText(RenderContext context, string text)
        {
            context.Append(text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]"));
        }

        private static void RenderUrl(RenderContext context, string url)
        {
            var chars = url.ToCharArray();
            var containsNonPrintable = chars.Any(c => char.IsWhiteSpace(c) || char.IsControl(c));
            if (containsNonPrintable)
            {
                context.Append('<');
                context.Append(url.Replace("\\", "\\\\").Replace("<", "\\>").Replace(">", "\\>"));
                context.Append('>');
            }
            else
            {
                context.Append(url.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)"));
            }
        }

        private static void RenderTitle(RenderContext context, string title)
        {
            // title can contain newlines. So use RenderIndented
            var chars = title.ToCharArray();
            var singleQuotes = chars.Count(c => c == '\'');
            var doubleQuotes = chars.Count(c => c == '\"');
            // We can use "()" for quoting, but we use only single/double quotes for the sake of simplicity.
            if (singleQuotes < doubleQuotes)
            {
                context.Append('\'');
                RenderIndented(context.WithNoIndentation(), title.Replace("'", "\\'"));
                context.Append('\'');
            }
            else
            {
                context.Append('"');
                RenderIndented(context.WithNoIndentation(), title.Replace("\"", "\\\""));
                context.Append('"');
            }
        }
    }

    internal sealed class CodeNode : MarkdownNode
    {
        public CodeNode(string value)
        {
            _value = value;
        }

        private readonly string _value;

        internal override bool SpaceBetweenInlines => true;

        internal override void RenderInternal(RenderContext context)
        {
            var quote = GetQuote(_value);
            var pad = _value.StartsWith("`") || _value.EndsWith("`");
            context.Append(quote);
            if (pad)
            {
                context.Append(' ');
            }

            RenderIndented(context.WithNoIndentation(), _value);
            if (pad)
            {
                context.Append(' ');
            }

            context.Append(quote);
        }

        /// <summary>
        /// Find shortest code span quote.
        /// According to the CommonMark spec, code span ends with a backtick string of equal length.
        /// So it finds a shortest backticks that does not appears in the code block
        /// </summary>
        /// <example>
        /// <code>
        /// GetQuote("` `` ````") == "```"
        /// </code>
        /// <code>
        /// GetQuote("`` ````") == "`"
        /// </code>
        /// </example>
        private static string GetQuote(string value)
        {
            var quoteLengthSet = new SortedSet<int>();

            foreach (Match quote in Regex.Matches(value, "`+"))
            {
                quoteLengthSet.Add(quote.Length);
            }

            if (quoteLengthSet.Count == 0)
            {
                return "`";
            }

            var quoteLengths = quoteLengthSet.ToList();
            var selectedLength = 1;
            foreach (var quoteLength in quoteLengths)
            {
                if (quoteLength != selectedLength)
                {
                    break;
                }

                selectedLength++;
            }

            return string.Join(string.Empty, Enumerable.Repeat("`", selectedLength));
        }
    }

    internal sealed class CodeBlockNode : MarkdownNode
    {
        public CodeBlockNode(string value)
        {
            _value = value;
        }

        private readonly string _value;

        public string Info { get; set; }

        internal override bool IsBlock => true;


        internal override void RenderInternal(RenderContext context)
        {
            var fence = GetFence(_value);
            context.IndentForFirstLine();
            context.Append(fence);
            if (Info != null)
            {
                context.Append(Info);
            }

            context.AppendLine();

            if (!string.IsNullOrEmpty(_value))
            {
                RenderIndented(context.WithIndentationForRest(), _value);
                context.AppendLine();
            }

            context.IndentForRest();
            context.Append(fence);
        }

        /// <summary>
        /// Find shortest code block fence.
        /// According to the CommonMark spec, code block ends with a fence of equal length.
        /// So it finds a shortest fence that does not appears in the code block
        /// </summary>
        private static string GetFence(string value)
        {
            var fenceLengthSet = new SortedSet<int>();
            var lines = value.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim(' ').ToCharArray();
                if (trimmed.Length < 3 || trimmed.Any(x => x != '`')) // Not a closing fence
                {
                    continue;
                }

                var spaces = line.Length - line.TrimStart(' ').Length;
                if (spaces >= 4) // It indented too much. It is not a fence.
                {
                    continue;
                }

                fenceLengthSet.Add(trimmed.Length);
            }

            if (fenceLengthSet.Count == 0)
            {
                return "```";
            }

            var fenceLengths = fenceLengthSet.ToList();
            var selectedLength = 3;
            foreach (var fenceLength in fenceLengths)
            {
                if (fenceLength != selectedLength)
                {
                    break;
                }

                selectedLength++;
            }

            return string.Join(string.Empty, Enumerable.Repeat("`", selectedLength));
        }
    }

    internal class NodeCollection<T> : MarkdownNode, IEnumerable<T>
        where T : MarkdownNode
    {
        public NodeCollection()
        {
            Nodes = new List<T>();
        }

        protected NodeCollection(IEnumerable<T> nodes)
        {
            Nodes = nodes.ToList();
        }

        protected readonly IList<T> Nodes;

        internal override void RenderInternal(RenderContext context)
        {
            DelimiterNode lastDelimiterNode = null;
            if (Nodes.Count > 0)
            {
                Nodes[0].RenderInternal(context);
                lastDelimiterNode = Nodes[0] as DelimiterNode;
            }

            foreach (var node in Nodes.Skip(1))
            {
                if ((lastDelimiterNode is NewlineNode) && node.IsInline)
                {
                    // HACK: compensate indentation for inline block right after the newline.
                    context.IndentForRest();
                    node.RenderInternal(context.WithNoIndentation());
                }
                else if (!(lastDelimiterNode is NewlineNode) || node is DelimiterNode)
                {
                    node.RenderInternal(context.WithNoIndentation());
                }
                else // IsBlock || Newline || HardNewline
                {
                    node.RenderInternal(context.WithIndentationForRest());
                }

                lastDelimiterNode = node as DelimiterNode;
            }
        }

        public void Add(T node)
        {
            Nodes.Add(node);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    internal abstract class DelimiterNode : MarkdownNode
    {
        public abstract DelimiterNode Normalize();
    }

    internal class SpaceNode : DelimiterNode
    {
        public static readonly SpaceNode None = new SpaceNode(0);
        public static readonly SpaceNode Default = new SpaceNode(1);

        public SpaceNode(int count)
        {
            _count = count;
        }

        private readonly int _count;

        internal override void RenderInternal(RenderContext context)
        {
            for (int i = 0; i < _count; i++)
            {
                context.Append(' ');
            }
        }

        public override DelimiterNode Normalize()
        {
            return new SpaceNode(Math.Min(1, _count));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is SpaceNode other)) return false;
            return _count == other._count;
        }

        public override int GetHashCode()
        {
            return _count;
        }
    }

    internal class NewlineNode : DelimiterNode, IComparable<NewlineNode>
    {
        public static readonly NewlineNode Default = new NewlineNode(1);
        public static readonly NewlineNode Hard = new NewlineNode(2);

        public NewlineNode(int count)
        {
            _count = count;
        }

        private readonly int _count;

        internal override void RenderInternal(RenderContext context)
        {
            for (int i = 0; i < _count; i++)
            {
                context.Append('\n');
            }
        }

        public override DelimiterNode Normalize()
        {
            return new NewlineNode(Math.Min(1, _count));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (!(obj is NewlineNode other)) return false;
            return _count == other._count;
        }

        public override int GetHashCode()
        {
            return _count;
        }

        public int CompareTo(NewlineNode other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return _count.CompareTo(other._count);
        }

        public static NewlineNode GetStrongest(NewlineNode a, NewlineNode b)
        {
            return a.CompareTo(b) > 0 ? a : b;
        }
    }

    internal abstract class ListNode : NodeCollection<ListItemNode>
    {
        internal override bool IsBlock => true;

        internal override NewlineNode DelimiterAfterBlock => NewlineNode.Hard;

        public abstract void Add(params MarkdownNode[] nodes);

        internal override void RenderInternal(RenderContext context)
        {
            if (Nodes.Count > 0)
            {
                Nodes[0].RenderInternal(context);
            }

            foreach (var node in Nodes.Skip(1))
            {
                context.AppendLine();
                node.RenderInternal(context.WithIndentationForRest());
            }
        }
    }

    internal sealed class BulletListNode : ListNode
    {
        public override void Add(params MarkdownNode[] nodes)
        {
            var node = new ListItemNode("* ", nodes);
            Nodes.Add(node);
        }
    }

    internal sealed class NumberedListNode : ListNode
    {
        public override void Add(params MarkdownNode[] nodes)
        {
            var label = Nodes.Count + 1;
            var node = new ListItemNode($"{label}. ", nodes);
            Nodes.Add(node);
        }
    }

    internal sealed class ListItemNode : NodeCollection<MarkdownNode>
    {
        public ListItemNode(string marker, params MarkdownNode[] nodes)
            : base(nodes)
        {
            _marker = marker;
        }

        private readonly string _marker;

        internal override void RenderInternal(RenderContext context)
        {
            context.IndentForFirstLine();
            context.Append(_marker);
            if (Nodes.Count > 0)
            {
                Nodes[0].RenderInternal(context.WithNoIndentation(_marker.Length));
            }

            foreach (var value in Nodes.Skip(1))
            {
                context.AppendLine();
                value.RenderInternal(context.WithIndentationForRest(_marker.Length));
            }
        }
    }

    internal sealed class TableNode : MarkdownNode
    {
        private readonly List<string> _labels = new List<string>();
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();
        private readonly List<Dictionary<string, NodeCollection<MarkdownNode>>> _rows =
            new List<Dictionary<string, NodeCollection<MarkdownNode>>>();

        internal override bool IsBlock => true;
        internal override NewlineNode DelimiterAfterBlock => NewlineNode.Hard;

        internal override void RenderInternal(RenderContext context)
        {
            if (_labels.Count == 0)
                return;

            context.IndentForFirstLine();
            context.Append("| ");
            context.Append(_headers[_labels[0]]);
            foreach (var label in _labels.Skip(1))
            {
                context.Append(" | ");
                context.Append(_headers[label]);
            }

            context.Append(" |");

            context.AppendLine();
            context.IndentForRest();
            context.Append('|');
            // Can't easily decide width of non-latin strings (e.g. CJK, emoji, NJW, etc.)
            context.Append(string.Join("|", Enumerable.Repeat("---", _labels.Count)));
            context.Append('|');

            foreach (var row in _rows)
            {
                context.AppendLine();
                context.IndentForRest();
                context.Append("| ");
                row[_labels[0]].RenderInternal(context.WithNoIndentation());
                foreach (var label in _labels.Skip(1))
                {
                    context.Append(" | ");
                    row[label].RenderInternal(context.WithNoIndentation());
                }

                context.Append(" |");
            }
        }

        public void AddHeader(string label, string header)
        {
            if (!_labels.Contains(label))
            {
                _labels.Add(label);
            }

            _headers.Add(label, header);
        }

        public IDictionary<string, NodeCollection<MarkdownNode>> CreateRow()
        {
            var row = new Dictionary<string, NodeCollection<MarkdownNode>>();
            _rows.Add(row);
            return row;
        }
    }
}