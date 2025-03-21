namespace Swashbuckle.AspNetCore.Annotations;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class SwaggerOperationFilterAttribute(Type filterType) : Attribute
{
    public Type FilterType { get; private set; } = filterType;
}
