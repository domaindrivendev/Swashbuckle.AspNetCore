namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-ClassWithXmlComments
public class ProductLine
{
    /// <summary>
    /// The name of the product
    /// </summary>
    /// <example>Men's basketball shoes</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Quantity left in stock
    /// </summary>
    /// <example>10</example>
    public int AvailableStock { get; set; }

    /// <summary>
    /// The sizes the product is available in
    /// </summary>
    /// <example>["Small", "Medium", "Large"]</example>
    public List<string> Sizes { get; set; } = [];
}
// end-snippet
