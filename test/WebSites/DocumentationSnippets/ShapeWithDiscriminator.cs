using System.Text.Json.Serialization;

namespace DocumentationSnippets;

// begin-snippet: Annotations-ShapeWithDiscriminator
[JsonPolymorphic(TypeDiscriminatorPropertyName = "shapeType")]
[JsonDerivedType(typeof(Rectangle), "rectangle")]
[JsonDerivedType(typeof(Circle), "circle")]
public abstract class ShapeWithDiscriminator
{
    // Avoid using a JsonPolymorphicAttribute.TypeDiscriminatorPropertyName
    // that conflicts with a property in your type hierarchy.
    // Related issue: https://github.com/dotnet/runtime/issues/72170
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShapeType
{
    Circle,
    Rectangle
}
// end-snippet
