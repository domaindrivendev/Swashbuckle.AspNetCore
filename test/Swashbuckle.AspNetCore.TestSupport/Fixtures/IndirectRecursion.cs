namespace Swashbuckle.AspNetCore.TestSupport.Fixtures
{
    public class IndirectRecursion
    {
        public IndirectRecursionIntermediary Intermediary { get; set; }
    }

    public class IndirectRecursionIntermediary
    {
        public IndirectRecursion Recursion { get; set; }
    }
}