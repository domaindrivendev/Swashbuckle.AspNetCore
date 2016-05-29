namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class ContainingType
    {
        public NestedType Property1 { get; set; }

        public class NestedType
        {
            public string Property2 { get; set; }
        }
    }
}