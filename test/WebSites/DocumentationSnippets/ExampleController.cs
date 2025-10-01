using Microsoft.AspNetCore.Mvc;

namespace DocumentationSnippets;

// begin-snippet: README-AttributeRouting
[Route("example")]
public class ExampleController : Controller
{
    [HttpGet("")]
    public IActionResult DoStuff()
    {
        // Your implementation
        return Empty;
    }
}
// end-snippet
