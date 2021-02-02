namespace Swashbuckle.AspNetCore.TestSupport
{
#nullable enable
    public class TypeWithNullableContext
    {
        public string? NullableString { get; set; }

        public string NonNullableString { get; set; }
    }
#nullable restore
}