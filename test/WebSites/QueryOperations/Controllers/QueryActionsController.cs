using Microsoft.AspNetCore.Mvc;

namespace QueryOperations.Controllers;

/// <summary>
/// Actions using the HTTP QUERY method, which is a first-class operation in OpenAPI 3.2
/// </summary>
[Route("/products")]
[Produces("application/json")]
public class QueryActionsController
{
    /// <summary>
    /// Searches the collection of products using a request body
    /// </summary>
    /// <param name="query">The search criteria</param>
    [AcceptVerbs("QUERY", Name = "QueryProducts")]
    [ProducesResponseType(typeof(ProductQueryResult), StatusCodes.Status200OK)]
    public IActionResult Query([FromBody] ProductQuery query)
    {
        return new OkObjectResult(new ProductQueryResult { Query = query });
    }

    /// <summary>
    /// Gets a product by its identifier
    /// </summary>
    /// <param name="id">The product identifier</param>
    [HttpGet("{id}", Name = "GetProduct")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    public IActionResult Get(int id)
    {
        return new OkObjectResult(new Product { Id = id });
    }

    /// <summary>
    /// Purges a product from the cache using a non-standard HTTP method
    /// </summary>
    /// <param name="id">The product identifier</param>
    [AcceptVerbs("PURGE", Route = "{id}", Name = "PurgeProduct")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Purge(int id)
    {
        return new NoContentResult();
    }
}

public class Product
{
    /// <summary>
    /// The product identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The product name
    /// </summary>
    public string Name { get; set; }
}

public class ProductQuery
{
    /// <summary>
    /// Keywords to search for
    /// </summary>
    public string Keywords { get; set; }

    /// <summary>
    /// The maximum number of results to return
    /// </summary>
    public int? Limit { get; set; }
}

public class ProductQueryResult
{
    /// <summary>
    /// The query that was executed
    /// </summary>
    public ProductQuery Query { get; set; }

    /// <summary>
    /// The identifiers of the matching products
    /// </summary>
    public int[] ProductIds { get; set; }
}
