#if NET
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class Issue3013Controller : ControllerBase
{
    [HttpGet]
    public TestResponse Get()
    {
        return new()
        {
            Foo = new(1, 2),
        };
    }

    public record TestResponse
    {
        public TestStruct? Foo { get; init; }
    }

    public record struct TestStruct(int A, int B);
}
#endif
