using Newtonsoft.Json;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class ComplexType
    {
        public bool Property1 { get; set; }

        public int Property2 { get; } = 123;

        public int Property3 { set { } }
    }
}