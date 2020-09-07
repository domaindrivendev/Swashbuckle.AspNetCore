using System;
using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.AspNetCore.TestSupport.Fixtures
{
    public class ExtendedRangeAttribute : RangeAttribute
    {
        public ExtendedRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public ExtendedRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        public ExtendedRangeAttribute(Type type, string minimum, string maximum) : base(type, minimum, maximum)
        {
        }
    }
}
