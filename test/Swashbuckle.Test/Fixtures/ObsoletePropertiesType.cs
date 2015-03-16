using System;

namespace Swashbuckle.Test.Fixtures
{
    public class ObsoletePropertiesType
    {
        public string Property1 { get; set; }

        [Obsolete]
        public string ObsoleteProperty { get; set; }
    }
}