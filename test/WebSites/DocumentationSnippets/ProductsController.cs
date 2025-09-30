using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

    // begin-snippet: Annotations-SwaggerOperation
    [HttpPost]
    [SwaggerOperation(
        Summary = "Creates a new product",
        Description = "Requires admin privileges",
        OperationId = "CreateProduct",
        Tags = ["Purchase", "Products"]
    )]
    public IActionResult Create([FromBody] Product product)
    {
        //...
        return Ok();
    }
    // end-snippet

    // begin-snippet: Annotations-SwaggerResponse
    [HttpPost]
    [SwaggerResponse(201, "The product was created", typeof(Product))]
    [SwaggerResponse(400, "The product data is invalid")]
    public IActionResult Post([FromBody] Product product)
    {
        //...
        return Created();
    }
    // end-snippet

    // begin-snippet: Annotations-SwaggerParameter
    [HttpGet]
    public IActionResult GetProducts(
        [FromQuery, SwaggerParameter("Search keywords", Required = true)] string keywords)
    {
        //...
        return Ok();
    }
    // end-snippet

    // begin-snippet: Annotations-SwaggerRequestBody
    [HttpPost]
    public IActionResult SubmitProduct(
        [FromBody, SwaggerRequestBody("The product payload", Required = true)] Product product)
    {
        //...
        return Created();
    }
    // end-snippet
}
