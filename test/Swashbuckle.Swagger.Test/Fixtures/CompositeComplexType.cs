using System.Collections.Generic;

namespace Swashbuckle.Fixtures
{
    public class CompositeComplexType
    {
        public ComplexType Property1 { get; set; }

        public IEnumerable<ComplexType> Property2 { get; set; }
    }
}