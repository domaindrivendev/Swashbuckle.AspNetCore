using System.Collections.Generic;

namespace Swashbuckle.Swagger.Fixtures
{
    public class NestedComplexType
    {
        public ComplexType Property1 { get; set; }

        public IEnumerable<ComplexType> Property2 { get; set; }
    }
}