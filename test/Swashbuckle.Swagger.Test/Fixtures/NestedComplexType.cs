namespace Swashbuckle.Fixtures
{
    public class NestedComplexType
    {
        public InnerType Property1 { get; set; }

        public class InnerType
        {
            public string Property2 { get; set; }
        }
    }
}