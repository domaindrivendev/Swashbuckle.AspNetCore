using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerGen;

public class DataProperty
{
    public DataProperty(
        string name,
        Type memberType,
        bool isRequired = false,
        bool isNullable = false,
        bool isReadOnly = false,
        bool isWriteOnly = false,
        MemberInfo memberInfo = null)
    {
        Name = name;
        IsRequired = isRequired;
        IsNullable = isNullable;
        IsReadOnly = isReadOnly;
        IsWriteOnly = isWriteOnly;
        MemberType = memberType;
        MemberInfo = memberInfo;
    }

    public string Name { get; } 
    public bool IsRequired { get; }
    public bool IsNullable { get; }
    public bool IsReadOnly { get; }
    public bool IsWriteOnly { get; }
    public Type MemberType { get; }
    public MemberInfo MemberInfo { get; }
}
