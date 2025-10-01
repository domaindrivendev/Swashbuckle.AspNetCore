using Swashbuckle.AspNetCore.Annotations;

namespace DocumentationSnippets;

// begin-snippet: Annotations-SchemaModel
[SwaggerSchemaFilter(typeof(ItemSchemaFilter))]
public class Item
{
    //...
}
// end-snippet
