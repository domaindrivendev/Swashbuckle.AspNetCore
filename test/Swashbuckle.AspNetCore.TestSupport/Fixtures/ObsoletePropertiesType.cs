using System;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class ObsoletePropertiesType
    {
        public string Property1 { get; set; }

        [Obsolete]
        public string ObsoleteProperty { get; set; }
    }
}