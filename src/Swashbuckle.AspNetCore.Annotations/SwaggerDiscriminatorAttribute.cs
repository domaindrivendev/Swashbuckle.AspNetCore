namespace Swashbuckle.AspNetCore.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public class SwaggerDiscriminatorAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; set; } = propertyName;
}
