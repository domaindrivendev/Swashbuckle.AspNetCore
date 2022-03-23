namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithRestrictedProperties
    {
        public int ReadWriteProperty { get; set; }
        public int ReadOnlyProperty { get; }
        public int WriteOnlyProperty { set { } }
    }
}
