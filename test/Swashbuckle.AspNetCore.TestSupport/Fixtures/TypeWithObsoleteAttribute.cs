using System;

namespace Swashbuckle.AspNetCore.TestSupport
{
    public class TypeWithObsoleteAttribute
    {
        [Obsolete]
        public string ObsoleteProperty { get; set; }
    }
}