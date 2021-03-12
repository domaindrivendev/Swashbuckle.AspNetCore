using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable
    public class TypeWithNullableContext
    {
        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; } = default!;
        public int[]? NullableArray { get; set; }
        public int[] NonNullableArray { get; set; } = default!;

        public List<SubTypeWithOneNullableContent>? NullableList { get; set; }
        public List<SubTypeWithOneNonNullableContent> NonNullableList { get; set; } = default!;

        public class SubTypeWithOneNullableContent
        {
            public string? NullableString { get; set; }
        }

        public class SubTypeWithOneNonNullableContent
        {
            public string NonNullableString { get; set; } = default!;
        }


    }
#nullable restore
}