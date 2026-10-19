#if NET11_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

/// <summary>
/// Actions using the HTTP QUERY method, which is a first-class operation in OpenAPI 3.2
/// </summary>
[Route("/products/query")]
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
#endif
