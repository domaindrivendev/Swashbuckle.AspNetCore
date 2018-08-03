using Microsoft.AspNetCore.Mvc;

namespace CliStartupAttributeExample
{
    public abstract class PrivateApiController : ControllerBase
    {
        protected PrivateApiController() { }
    }
}
