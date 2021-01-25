using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable
    public class FixedNonNullableReferenceType
    {
        public string? NullableStringValue { get; set; }

        public string NonNullableStringValue { get; set; }
    }
#nullable restore
}