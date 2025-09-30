using Swashbuckle.AspNetCore.Annotations;

namespace DocumentationSnippets;

// begin-snippet: Annotations-SwaggerSchema
[SwaggerSchema(Required = ["Description"])]
public class Product
{
    [SwaggerSchema("The product identifier", ReadOnly = true)]
    public int Id { get; set; }

    [SwaggerSchema("The product description")]
    public string Description { get; set; } = string.Empty;

    [SwaggerSchema("The date it was created", Format = "date")]
    public DateTime DateCreated { get; set; }
}
// end-snippet
