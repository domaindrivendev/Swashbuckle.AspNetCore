namespace Swashbuckle.AspNetCore.SwaggerGen;

public class FilterDescriptor
{
    public Type Type { get; set; }

    public object[] Arguments { get; set; }

    public object FilterInstance { get; set; }

    internal bool IsAssignableTo(Type type)
    {
        return (FilterInstance != null && type.IsInstanceOfType(FilterInstance)) ||
               (Type != null && Type.IsAssignableTo(type));
    }
}
