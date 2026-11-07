namespace Swashbuckle.AspNetCore.TestSupport;

public class GenericSubTypesBaseType
{
    public string BaseProperty { get; set; }
}

public class OpenGenericSubType<T> : GenericSubTypesBaseType
{
    public T Details { get; set; }
}

public class ClosedGenericSubType : OpenGenericSubType<int>;
