namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class ComplexTypeWithConstructor
    {
        public ComplexTypeWithConstructor(string property1)
        {
            Property1 = property1;
        }

        public string Property1 { get; }
    }
}
