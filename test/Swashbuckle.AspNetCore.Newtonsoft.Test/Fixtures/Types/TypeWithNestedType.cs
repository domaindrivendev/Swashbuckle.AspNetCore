namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class TypeWithNestedType
    {
        public NestedType Property1 { get; set; }

        public class NestedType
        {
            public string Property1 { get; set; }
        }
    }
}