using System;
using Microsoft.AspNetCore.Mvc;
using XmlComments.Models;

namespace XmlComments.Controllers
{
    /// <summary>
    /// Product operations
    /// </summary>
    [ApiController]
    [Route("products")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        /// <summary>
        /// Creates a new product
        /// </summary>
        /// <remarks>
        /// Returns a unique id for the product
        /// </remarks>
        /// <param name="product">The product info</param>
        /// <response code="200">Product created</response>
        /// <response code="400">Invalid product info</response>
        [HttpPost]
        public int CreateProduct([FromBody]Product product)
        {
            throw new NotImplementedException();
        }
    }
}
