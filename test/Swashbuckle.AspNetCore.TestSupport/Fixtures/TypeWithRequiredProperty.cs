namespace Swashbuckle.AspNetCore.TestSupport.Fixtures
{
    public class TypeWithRequiredProperty
    {
#if NET7_0_OR_GREATER
        public required string RequiredProperty { get; set; }
#endif
    }
}
