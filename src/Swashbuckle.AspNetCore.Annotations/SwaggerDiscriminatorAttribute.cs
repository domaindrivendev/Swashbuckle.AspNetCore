namespace Swashbuckle.AspNetCore.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public class SwaggerDiscriminatorAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; set; } = propertyName;

    /// <summary>
    /// Gets or sets the type whose schema should be used to validate a payload when the discriminator property
    /// is not present, or has a value that is not in the discriminator mapping. This is emitted as the
    /// <c>defaultMapping</c> of the discriminator, which is only supported by OpenAPI 3.2.
    /// </summary>
    public Type DefaultMapping { get; set; }
}
