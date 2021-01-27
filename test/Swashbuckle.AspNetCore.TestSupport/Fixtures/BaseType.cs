namespace Swashbuckle.AspNetCore.TestSupport
{
    public class BaseType
    {
        public string BaseProperty { get; set; }
    }

    public class SubType1 : BaseType
    {
        public int Property1 { get; set; }
    }

    public class SubType2 : BaseType
    {
        public int Property2 { get; set; }
    }
}