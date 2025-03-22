namespace Swashbuckle.AspNetCore.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class SwaggerSubTypeAttribute(Type subType) : Attribute
{
    public Type SubType { get; set; } = subType;

    public string DiscriminatorValue { get; set; }
}
