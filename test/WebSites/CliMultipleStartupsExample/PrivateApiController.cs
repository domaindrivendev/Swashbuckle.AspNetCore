using Microsoft.AspNetCore.Mvc;

namespace CliMultipleStartupsExample
{
    public abstract class PrivateApiController : ControllerBase
    {
        protected PrivateApiController() { }
    }
}
