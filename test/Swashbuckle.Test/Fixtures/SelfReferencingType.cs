using System;

namespace Swashbuckle.Fixtures
{
    public class SelfReferencingType
    {
        public SelfReferencingType Another { get; set; }
    }
}