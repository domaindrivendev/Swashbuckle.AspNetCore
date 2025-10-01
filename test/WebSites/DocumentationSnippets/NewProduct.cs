using System.ComponentModel.DataAnnotations;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-NewProduct
public class NewProduct
{
    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
// end-snippet
