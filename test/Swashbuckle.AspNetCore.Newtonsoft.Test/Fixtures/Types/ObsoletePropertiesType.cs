using System;

namespace Swashbuckle.AspNetCore.Newtonsoft.Test
{
    public class ObsoletePropertiesType
    {
        public string Property1 { get; set; }

        [Obsolete]
        public string ObsoleteProperty { get; set; }
    }
}