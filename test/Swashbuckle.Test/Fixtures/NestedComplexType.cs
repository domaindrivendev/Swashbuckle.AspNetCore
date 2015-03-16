using System.Collections.Generic;

namespace Swashbuckle.Test.Fixtures
{
    public class NestedComplexType
    {
        public ComplexType Property1 { get; set; }

        public IEnumerable<ComplexType> Property2 { get; set; }
    }
}