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

    public class AllNonNullableReferenceType
    {
        public string Property1 { get; set; }
    }

    public class AllNullableReferenceType
    {
        public string? Property1 { get; set; }
    }
#nullable restore
}
