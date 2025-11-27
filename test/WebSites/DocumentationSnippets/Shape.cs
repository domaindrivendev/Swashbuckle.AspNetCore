using System.Text.Json.Serialization;

namespace DocumentationSnippets;

// begin-snippet: Annotations-Shape
[JsonDerivedType(typeof(Rectangle))]
[JsonDerivedType(typeof(Circle))]
public abstract class Shape
{
}
// end-snippet
