using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class DataProperty(
    string name,
    Type memberType,
    bool isRequired = false,
    bool isNullable = false,
    bool isReadOnly = false,
    bool isWriteOnly = false,
    MemberInfo memberInfo = null)
{
    public string Name { get; } = name;
    public bool IsRequired { get; } = isRequired;
    public bool IsNullable { get; } = isNullable;
    public bool IsReadOnly { get; } = isReadOnly;
    public bool IsWriteOnly { get; } = isWriteOnly;
    public Type MemberType { get; } = memberType;
    public MemberInfo MemberInfo { get; } = memberInfo;
}
