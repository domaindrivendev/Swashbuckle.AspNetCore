using Microsoft.AspNetCore.Mvc;

namespace DocumentationSnippets;

public class ProductsController : Controller
{
    // begin-snippet: README-endpoints
    [HttpPost]
    public void CreateProduct([FromBody] Product product)
    {
        // Implementation goes here
    }

    [HttpGet]
    public IEnumerable<Product> SearchProducts([FromQuery] string keywords)
    {
        // Implementation goes here
        return [];
    }
    // end-snippet
}
