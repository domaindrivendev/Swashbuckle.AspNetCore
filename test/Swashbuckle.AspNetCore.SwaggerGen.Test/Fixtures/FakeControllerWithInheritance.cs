using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

public class FakeControllerWithInheritance
{
    public void ActionWithDerivedObjectParameter([FromBody] AbcTests_C param)
    { }

    public List<AbcTests_A> ActionWithDerivedObjectResponse()
    {
        return null!;
    }

    public AbcTests_B ActionWithDerivedObjectResponse_ExcludedFromInheritanceConfig()
    {
        return null!;
    }

    // Helper test types for GenerateSchema_PreservesIntermediateBaseProperties_WhenUsingOneOfPolymorphism
    public abstract class AbcTests_A
    {
        public string PropA { get; set; }
    }

    public class AbcTests_B : AbcTests_A
    {
        public string PropB { get; set; }
    }

    public class AbcTests_C : AbcTests_B
    {
        public string PropC { get; set; }
    }
}
