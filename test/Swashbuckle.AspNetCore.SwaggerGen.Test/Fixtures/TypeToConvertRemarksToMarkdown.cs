namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class TypeToConvertRemarksToMarkdown
    {
        /// <summary>
        /// </summary>
        public static string Empty = @"";

        /// <summary>
        /// Plaintext
        /// </summary>
        public static string Plaintext = @"Plaintext";

        /// <summary>
        /// <see cref="System.String"/>
        /// </summary>
        public static string SingleElement = @"`System.String`";

        /// <summary>
        /// This <see cref="System.String"/> should be a code.
        /// This <see langword="null"/> is also a code.
        /// This <see href="http://github.com"/> is an autolink.
        /// <a href="http://github.com" title="Github">This</a> is a link.
        /// </summary>
        public static string Links = @"This `System.String` should be a code.
This `null` is also a code.
This <http://github.com> is an autolink.
[This](http://github.com ""Github"") is a link.";

        /// <summary>
        /// <para>
        /// Inline <c>code</c> is usually doesn't contain backticks.
        /// Whenever it contains backticks, <c>It should be ` quoted ``` with more backticks</c>
        /// </para>
        /// <para>
        /// Markdown allows <c>newline
        ///        characters and spaces in inline code blocks!</c>
        /// Although they are joined while they are rendered.
        /// </para>
        /// <para>
        /// newlines <c>in a code block
        ///
        ///     should be
        /// </c> normalized
        /// </para>
        /// </summary>
        public static string InlineCode = @"Inline `code` is usually doesn't contain backticks.
Whenever it contains backticks, ``It should be ` quoted ``` with more backticks``

Markdown allows `newline
       characters and spaces in inline code blocks!`
Although they are joined while they are rendered.

newlines `in a code block
    should be` normalized";

        /// <summary>
        /// <code lang="csharp">
        ///    for (var i=0; i&lt;10; i++)
        ///    {
        ///      Console.WriteLine("Hello, World!");
        ///    }
        /// </code>
        ///
        /// <code>
        ///    Lang is an optional
        /// </code>
        /// </summary>
        public static string CodeBlock = @"```csharp
for (var i=0; i<10; i++)
{
  Console.WriteLine(""Hello, World!"");
}
```

```
Lang is an optional
```";

        /// <summary>
        ///   <para> A para block makes block paragraph node.</para>
        ///   <para>
        ///      Subsequent paragraphs make hard newlines.
        ///   </para>
        /// </summary>
        public static string Para = @"A para block makes block paragraph node.

Subsequent paragraphs make hard newlines.";

        /// <summary>
        ///   <list type="table">
        ///     <listheader>
        ///       <term>Term</term>
        ///       <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///       <term>Some term</term>
        ///       <description>Some description with <c>code</c></description>
        ///     </item>
        ///   </list>
        /// </summary>
        public static string Table = @"| Term | Description |
|---|---|
| Some term | Some description with `code` |";

        /// <summary>
        /// Top level should preserve the format of the input so
        /// 1. Should have 2 newlines:
        /// <para>hard block</para><para>hard block</para>
        /// 2. Should have 2 newlines:
        /// <para>hard block</para> <para>hard block</para>
        /// 3. Should have 2 newlines:
        /// <para>hard block</para>  <para>hard block</para>
        /// 4. Should have 2 newlines:
        /// <para>hard block</para>
        /// <para>hard block</para>
        /// 5. Should have 2 newlines:
        /// <para>hard block</para>
        ///
        /// <para>hard block</para>
        /// 6. Should have 3 newlines:
        /// <para>hard block</para>
        ///
        ///
        /// <para>hard block</para>
        /// </summary>
        public static string HardBlocks = @"Top level should preserve the format of the input so
1. Should have 2 newlines:

hard block

hard block

2. Should have 2 newlines:

hard block

hard block

3. Should have 2 newlines:

hard block

hard block

4. Should have 2 newlines:

hard block

hard block

5. Should have 2 newlines:

hard block

hard block

6. Should have 3 newlines:

hard block


hard block";

        /// <summary>
        /// Inside of the Xml node, nodes should be normalized to be a valid markdown.
        /// <para>
        /// 1. Should have 2 newlines:
        /// <para>hard block</para><para>hard block</para>
        /// 2. Should have 2 newlines:
        /// <para>hard block</para> <para>hard block</para>
        /// 3. Should have 2 newlines:
        /// <para>hard block</para>  <para>hard block</para>
        /// 4. Should have 2 newlines:
        /// <para>hard block</para>
        /// <para>hard block</para>
        /// 5. Should have 2 newlines:
        /// <para>hard block</para>
        ///
        /// <para>hard block</para>
        /// 6. Should have 2 newlines:
        /// <para>hard block</para>
        ///
        ///
        /// <para>hard block</para>
        /// </para>
        /// </summary>
        public static string HardBlocksInXml =
            @"Inside of the Xml node, nodes should be normalized to be a valid markdown.

1. Should have 2 newlines:

hard block

hard block

2. Should have 2 newlines:

hard block

hard block

3. Should have 2 newlines:

hard block

hard block

4. Should have 2 newlines:

hard block

hard block

5. Should have 2 newlines:

hard block

hard block

6. Should have 2 newlines:

hard block

hard block";

        /// <summary>
        /// Top level should preserve the format of the input so
        /// 1. Should have 1 newline:
        /// <code>soft block</code><code>soft block</code>
        /// 2. Should have 1 newline:
        /// <code>soft block</code> <code>soft block</code>
        /// 3. Should have 1 newline:
        /// <code>soft block</code>  <code>soft block</code>
        /// 4. Should have 1 newline:
        /// <code>soft block</code>
        /// <code>soft block</code>
        /// 5. Should have 2 newlines:
        /// <code>soft block</code>
        ///
        /// <code>soft block</code>
        /// 6. Should have 3 newlines:
        /// <code>soft block</code>
        ///
        ///
        /// <code>soft block</code>
        /// </summary>
        public static string SoftBlocks = @"Top level should preserve the format of the input so
1. Should have 1 newline:
```
soft block
```
```
soft block
```
2. Should have 1 newline:
```
soft block
```
```
soft block
```
3. Should have 1 newline:
```
soft block
```
```
soft block
```
4. Should have 1 newline:
```
soft block
```
```
soft block
```
5. Should have 2 newlines:
```
soft block
```

```
soft block
```
6. Should have 3 newlines:
```
soft block
```


```
soft block
```";

        /// <summary>
        /// <para>
        /// Inside of the Xml node, nodes should be normalized to be a valid markdown.
        /// 1. Should have 1 newline:
        /// <code>soft block</code><code>soft block</code>
        /// 2. Should have 1 newline:
        /// <code>soft block</code> <code>soft block</code>
        /// 3. Should have 1 newline:
        /// <code>soft block</code>  <code>soft block</code>
        /// 4. Should have 1 newline:
        /// <code>soft block</code>
        /// <code>soft block</code>
        /// 5. Should have 1 newline:
        /// <code>soft block</code>
        ///
        /// <code>soft block</code>
        /// 6. Should have 1 newline:
        /// <code>soft block</code>
        ///
        ///
        /// <code>soft block</code>
        /// </para>
        /// </summary>
        public static string SoftBlocksInXml =
            @"Inside of the Xml node, nodes should be normalized to be a valid markdown.
1. Should have 1 newline:
```
soft block
```
```
soft block
```
2. Should have 1 newline:
```
soft block
```
```
soft block
```
3. Should have 1 newline:
```
soft block
```
```
soft block
```
4. Should have 1 newline:
```
soft block
```
```
soft block
```
5. Should have 1 newline:
```
soft block
```
```
soft block
```
6. Should have 1 newline:
```
soft block
```
```
soft block
```";

        /// <summary>
        /// Top level should preserve the format of the input so
        /// 1. Should have 1 space:
        /// <c>inline</c><c>inline</c>
        /// 2. Should have 1 space:
        /// <c>inline</c> <c>inline</c>
        /// 3. Should have 1 space:
        /// <c>inline</c>  <c>inline</c>
        /// 4. Should have 1 newline:
        /// <c>inline</c>
        /// <c>inline</c>
        /// 5. Should have 2 newlines:
        /// <c>inline</c>
        ///
        /// <c>inline</c>
        /// 6. Should have 3 newlines:
        /// <c>inline</c>
        ///
        ///
        /// <c>inline</c>
        /// </summary>
        public static string Inilnes = @"Top level should preserve the format of the input so
1. Should have 1 space:
`inline` `inline`
2. Should have 1 space:
`inline` `inline`
3. Should have 1 space:
`inline` `inline`
4. Should have 1 newline:
`inline`

`inline`
5. Should have 2 newlines:
`inline`

`inline`
6. Should have 3 newlines:
`inline`


`inline`";

        /// <summary>
        /// <para>
        /// Inside of the Xml node, nodes should be normalized to be a valid markdown.
        /// 1. Should have 1 space:
        /// <c>inline</c><c>inline</c>
        /// 2. Should have 1 space:
        /// <c>inline</c> <c>inline</c>
        /// 3. Should have 1 space:
        /// <c>inline</c>  <c>inline</c>
        /// 4. Should have 1 newline:
        /// <c>inline</c>
        /// <c>inline</c>
        /// 5. Should have 1 newline:
        /// <c>inline</c>
        ///
        /// <c>inline</c>
        /// 6. Should have 1 newline:
        /// <c>inline</c>
        ///
        ///
        /// <c>inline</c>
        /// </para>
        /// </summary>
        public static string InlinesInXml = @"Inside of the Xml node, nodes should be normalized to be a valid markdown.
1. Should have 1 space:
`inline` `inline`
2. Should have 1 space:
`inline` `inline`
3. Should have 1 space:
`inline` `inline`
4. Should have 1 newline:
`inline`
`inline`
5. Should have 1 newline:
`inline`
`inline`
6. Should have 1 newline:
`inline`
`inline`";

        /// <summary>
        ///   <para>hard block</para><code>soft block</code>
        ///   <para>hard block</para><c>inline</c>
        ///
        ///   <code>soft block</code><para>hard block</para>
        ///   <code>soft block</code><c>inline</c>
        ///
        ///   <c>inline</c><para>hard block</para>
        ///   <c>inline</c><code>soft block</code>
        /// </summary>
        public static string AdjacentBlocksAndInlines = @"hard block

```
soft block
```
hard block

`inline`

```
soft block
```
hard block

```
soft block
```
`inline`

`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        /// <para>
        ///   <para>hard block</para><code>soft block</code>
        ///   <para>hard block</para><c>inline</c>
        ///
        ///   <code>soft block</code><para>hard block</para>
        ///   <code>soft block</code><c>inline</c>
        ///
        ///   <c>inline</c><para>hard block</para>
        ///   <c>inline</c><code>soft block</code>
        /// </para>
        /// </summary>
        public static string AdjacentBlocksAndInlinesInXml = @"hard block

```
soft block
```
hard block

`inline`
```
soft block
```
hard block

```
soft block
```
`inline`
`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        ///   <para>hard block</para> <code>soft block</code>
        ///   <para>hard block</para> <c>inline</c>
        ///
        ///   <code>soft block</code> <para>hard block</para>
        ///   <code>soft block</code> <c>inline</c>
        ///
        ///   <c>inline</c> <para>hard block</para>
        ///   <c>inline</c> <code>soft block</code>
        /// </summary>
        public static string AdjacentBySpaceBlocksAndInlines = @"hard block

```
soft block
```
hard block

`inline`

```
soft block
```
hard block

```
soft block
```
`inline`

`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        /// <para>
        ///   <para>hard block</para> <code>soft block</code>
        ///   <para>hard block</para> <c>inline</c>
        ///
        ///   <code>soft block</code> <para>hard block</para>
        ///   <code>soft block</code> <c>inline</c>
        ///
        ///   <c>inline</c> <para>hard block</para>
        ///   <c>inline</c> <code>soft block</code>
        /// </para>
        /// </summary>
        public static string AdjacentBySpaceBlocksAndInlinesInXml = @"hard block

```
soft block
```
hard block

`inline`
```
soft block
```
hard block

```
soft block
```
`inline`
`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        /// <para>hard block</para>
        /// <code>soft block</code>
        ///
        /// <para>hard block</para>
        /// <c>inline</c>
        ///
        ///
        /// <code>soft block</code>
        /// <para>hard block</para>
        ///
        /// <code>soft block</code>
        /// <c>inline</c>
        ///
        ///
        /// <c>inline</c>
        /// <para>hard block</para>
        ///
        /// <c>inline</c>
        /// <code>soft block</code>
        /// </summary>
        public static string AdjacentByNewlineBlocksAndInlines = @"hard block

```
soft block
```

hard block

`inline`


```
soft block
```
hard block

```
soft block
```
`inline`


`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        /// <para>
        /// <para>hard block</para>
        /// <code>soft block</code>
        ///
        /// <para>hard block</para>
        /// <c>inline</c>
        ///
        ///
        /// <code>soft block</code>
        /// <para>hard block</para>
        ///
        /// <code>soft block</code>
        /// <c>inline</c>
        ///
        ///
        /// <c>inline</c>
        /// <para>hard block</para>
        ///
        /// <c>inline</c>
        /// <code>soft block</code>
        /// </para>
        /// </summary>
        public static string AdjacentByNewlineBlocksAndInlinesInXml = @"hard block

```
soft block
```
hard block

`inline`
```
soft block
```
hard block

```
soft block
```
`inline`
`inline`

hard block

`inline`
```
soft block
```";

        /// <summary>
        /// Markdown support within remarks!
        ///
        /// * OpenAPI 2 and 3 supports Markdown in `description`
        /// * So you should be able to write documents in Markdown
        ///     * Now you can write Markdown with NSwag!
        ///     *   <list type="bullet">
        ///         <item>You can mix <c>XML</c> in <c>Markdown</c>
        ///         </item>
        ///         <item>It's cool</item>
        ///         <item>
        ///            Even you can
        ///            <list type="number">
        ///              <item>nest</item>
        ///              <item>items</item>
        ///            </list>
        ///            yay!
        ///         </item>
        ///         <item>
        ///         <code>
        ///           you can even
        ///           put code here
        ///         </code>
        ///         </item>
        ///       </list>
        ///     * return to the markdown
        /// </summary>
        public static string MixedXmlAndMarkdown = @"Markdown support within remarks!

* OpenAPI 2 and 3 supports Markdown in `description`
* So you should be able to write documents in Markdown
    * Now you can write Markdown with NSwag!
    *   * You can mix `XML` in `Markdown`
        * It's cool
        * Even you can
          1. nest
          2. items

          yay!
        * ```
          you can even
          put code here
          ```

    * return to the markdown";

        /// <summary>
        /// *   <list><item>blah</item></list>text
        /// * blah
        ///     <list><item>blah</item></list>text
        ///
        /// *   <list><item>blah</item></list>
        /// text
        /// * blah
        ///     <list><item>blah</item></list>
        /// text
        /// </summary>
        public static string Ambiguous1 = @"*   1. blah

    text
* blah
    1. blah

    text

*   1. blah

text
* blah
    1. blah

text";

        /// <summary>
        /// *   <para>para</para>text
        /// * blah
        ///     <para>para</para>text
        ///
        /// *   <para>para</para>
        /// text
        /// * blah
        ///     <para>para</para>
        /// text
        /// </summary>
        public static string Ambiguous2 = @"*   para

    text
* blah

    para

    text

*   para

text
* blah

    para

text";

        /// <summary>
        /// *   <c>inline</c>text
        /// * blah
        ///     <c>inline</c>text
        ///
        /// *   <c>inline</c>
        /// text
        /// * blah
        ///     <c>inline</c>
        /// text
        /// </summary>
        public static string Ambiguous3 = @"* `inline`text
* blah
    `inline`text

* `inline`
text
* blah
    `inline`
text";


        /// <summary>
        /// *   <list><item>list</item></list>
        ///     <list type="bullet"><item>bullet</item></list><list><item>list</item></list>
        /// </summary>
        public static string ListBullet = @"*   1. list

    * bullet

    1. list";
    }
}