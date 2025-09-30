using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

    // begin-snippet: SwaggerGen-NamedRoute
    // operationId = "GetProductById"
    [HttpGet("{id}", Name = "GetProductById")]
    public IActionResult Get(int id)
    {
        // ...
        return Ok();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-CustomNamingStrategyEndpoint
    // operationId = "GetProductById"
    [HttpGet("/product/{id}")]
    public IActionResult GetProductById(int id)
    {
        // ...
        return Ok();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-ImplicitResponse
    [HttpPost("{id}")]
    public Product GetById(int id)
    {
        // ...
        return new Product();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-ExplicitReponses
    [HttpPost("product/{id}")]
    [ProducesResponseType(typeof(Product), 200)]
    [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
    [ProducesResponseType(500)]
    public IActionResult GetProductInfoById(int id)
    {
        // ...
        return Ok();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-RequiredParametersEndpoint
    public IActionResult Search([FromQuery, BindRequired] string keywords, [FromQuery] PagingOptions paging)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // ...
        return Ok();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-RequiredParametersFromBody
    public IActionResult CreateNewProduct([FromBody] NewProduct product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // ...
        return Created();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-UploadFile
    [HttpPost]
    public void UploadFile([FromForm] string description, [FromForm] DateTime clientDate, IFormFile file)
    {
        // ...
    }
    // end-snippet

    // begin-snippet: SwaggerGen-DownloadFile
    [HttpGet("download/{fileName}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, "image/jpeg")]
    public FileStreamResult GetImage(string fileName)
    {
        // ...
        return new FileStreamResult(Stream.Null, "image/jpeg");
    }
    // end-snippet

    // begin-snippet: SwaggerGen-EndpointWithXmlComments
    /// <summary>
    /// Retrieves a specific product line by unique id
    /// </summary>
    /// <remarks>Awesomeness!</remarks>
    /// <param name="id" example="123">The product line id</param>
    /// <response code="200">Product line retrieved</response>
    /// <response code="404">Product line not found</response>
    /// <response code="500">Oops! Can't lookup your product line right now</response>
    [HttpGet("product/{id}")]
    [ProducesResponseType(typeof(ProductLine), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public ProductLine GetProductBySystemId(int id)
    {
        // ...
        return new ProductLine();
    }
    // end-snippet

    // begin-snippet: SwaggerGen-EndpointGroupName
    [HttpPost]
    [ApiExplorerSettings(GroupName = "v2")]
    public void PostLine([FromBody] ProductLine product)
    {
        // ...
    }
    // end-snippet

    // begin-snippet: SwaggerGen-HiddenByAttribute
    [HttpDelete("{id}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public void Delete(int id)
    {
        // ...
    }
    // end-snippet
}
