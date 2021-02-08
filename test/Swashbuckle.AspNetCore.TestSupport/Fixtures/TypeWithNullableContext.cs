namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable
    public class TypeWithNullableContext
    {
        public int? NullableInt { get; set; }
        public int NonNullableInt { get; set; }
        public string? NullableString { get; set; }
        public string NonNullableString { get; set; }
        public int[]? NullableArray { get; set; }
        public int[] NonNullableArray { get; set; }
    }
#nullable restore
}