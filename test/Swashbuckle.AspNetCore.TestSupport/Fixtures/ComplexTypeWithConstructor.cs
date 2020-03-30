namespace Swashbuckle.AspNetCore.TestSupport
{
    public class ComplexTypeWithConstructor
    {
        public ComplexTypeWithConstructor(bool property1)
        {
            Property1 = property1;
        }

        public bool Property1 { get; }
    }
}
