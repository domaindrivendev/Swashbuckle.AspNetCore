namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithOverriddenProperty : ComplexType
    {
        public new string Property1 { get; set; }
    }
}
