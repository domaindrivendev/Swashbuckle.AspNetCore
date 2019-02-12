namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class ContainingType
    {
        public NestedType Property1 { get; set; }

        public class NestedType
        {
            public string Property1 { get; set; }
        }
    }
}