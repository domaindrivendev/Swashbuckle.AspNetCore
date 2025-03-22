namespace Swashbuckle.AspNetCore.Annotations;

[AttributeUsage(
    AttributeTargets.Class |
    AttributeTargets.Struct |
    AttributeTargets.Enum,
    AllowMultiple = false)]
public class SwaggerSchemaFilterAttribute(Type type) : Attribute
{
    public Type Type { get; private set; } = type;

    public object[] Arguments { get; set; } = [];
}
