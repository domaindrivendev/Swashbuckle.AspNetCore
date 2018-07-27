using Microsoft.AspNetCore.Mvc;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures
{
    public abstract class PrivateApiController : ControllerBase
    {
        protected PrivateApiController() { }
    }
}
