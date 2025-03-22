namespace Swashbuckle.AspNetCore.Annotations;

[Obsolete("Use multiple SwaggerSubType attributes instead")]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
public class SwaggerSubTypesAttribute(params Type[] subTypes) : Attribute
{
    public IEnumerable<Type> SubTypes { get; } = subTypes;

    public string Discriminator { get; set; }
}
