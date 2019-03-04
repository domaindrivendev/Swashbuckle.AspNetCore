namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    public class ComplexType
    {
        public bool Property1 { get; set; }

        public int Property2 { get; set; }

        public string Property3 { get; set; }

        public string Property4 { get; }

        public string Property5 { set { } }

        public IntEnum Property6 { get; set; } = IntEnum.Value4;

        public IntEnum? Property7 { get; set; }

        public bool? Property8 { get; set; }

        public int? Property9 { get; set; }
    }
}