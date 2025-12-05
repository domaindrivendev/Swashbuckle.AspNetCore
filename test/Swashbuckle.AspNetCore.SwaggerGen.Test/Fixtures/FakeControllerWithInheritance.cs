using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test;

#pragma warning disable CA1822

public class FakeControllerWithInheritance : Controller
{
    public void ActionWithDerivedObjectParameter([FromBody] MostDerivedClass param)
    {
        System.Diagnostics.Debug.Assert(param is not null);
    }

    public List<BaseClass> ActionWithDerivedObjectResponse()
    {
        return null!;
    }

    public DerivedClass ActionWithDerivedObjectResponse_ExcludedFromInheritanceConfig()
    {
        return null!;
    }

    public abstract class BaseClass
    {
        public string BaseProperty { get; set; }
    }

    public class DerivedClass : BaseClass
    {
        public string DerivedProperty { get; set; }
    }

    public class MostDerivedClass : DerivedClass
    {
        public string MoreDerivedProperty { get; set; }
    }
}
