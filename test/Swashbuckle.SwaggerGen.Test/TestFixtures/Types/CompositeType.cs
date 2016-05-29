using System.Collections.Generic;

namespace Swashbuckle.SwaggerGen.TestFixtures
{
    public class CompositeType
    {
        public ComplexType Property1 { get; set; }

        public IEnumerable<ComplexType> Property2 { get; set; }
    }
}