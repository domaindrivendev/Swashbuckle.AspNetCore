using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DocumentationSnippets;

// begin-snippet: SwaggerGen-TagDescriptionsDocumentFilter
public class TagDescriptionsDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Tags =
        [
            new OpenApiTag { Name = "Products", Description = "Browse/manage the product catalog" },
            new OpenApiTag { Name = "Orders", Description = "Submit orders" }
        ];
    }
}
// end-snippet
