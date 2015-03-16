using System;

namespace Swashbuckle.Test.Fixtures
{
    public class SelfReferencingType
    {
        public SelfReferencingType Another { get; set; }
    }
}