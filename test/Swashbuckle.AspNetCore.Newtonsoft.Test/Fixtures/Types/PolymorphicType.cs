namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public abstract class PolymorphicType
    {
        public string BaseProperty { get; set; }
    }

    public class SubType1 : PolymorphicType
    {
        public int Property1 { get; set; }
    }

    public class SubType2 : PolymorphicType
    {
        public int Property2 { get; set; }
    }
}