namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class TypeWithOverriddenProperty : ComplexType
    {
        public new string Property1 { get; set; }
    }
}
