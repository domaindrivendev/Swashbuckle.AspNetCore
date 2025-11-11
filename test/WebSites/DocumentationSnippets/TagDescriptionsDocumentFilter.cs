using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-TagDescriptionsDocumentFilter
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags = new HashSet<OpenApiTag>()
        {
            new() { Name = "Products", Description = "Browse/manage the product catalog" },
            new() { Name = "Orders", Description = "Submit orders" }
        };
    }
}
// end-snippet
