namespace Swashbuckle.AspNetCore.TestSupport;

public class BaseSecondLevelType
{
    public string BaseProperty { get; set; }
}

public class SubSecondLevelType : BaseSecondLevelType
{
    public int Property1 { get; set; }
}

public class SubSubSecondLevelType : SubSecondLevelType
{
    public int Property2 { get; set; }
}
