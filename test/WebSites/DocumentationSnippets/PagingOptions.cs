using System.ComponentModel.DataAnnotations;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-RequiredParametersModel
public class PagingOptions
{
    [Required]
    public int PageNumber { get; set; }

    public int PageSize { get; set; }
}
// end-snippet
