using Microsoft.AspNetCore.Http;

namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    /// <summary>
    /// Summary for FakeControllerWithCustomTag
    /// </summary>
    [Tags("fake controller custom tag")]
    public class FakeControllerWithCustomTag
    {
        public void ActionAny()
        { }

        public void ActionAnother()
        { }
    }
}