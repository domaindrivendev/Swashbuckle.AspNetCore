using Microsoft.AspNetCore.Mvc;

namespace Filters.Controllers
{
    /// <summary>
    /// These endpoints are used to test that the document name will be carried
    /// through to filters so that filters can be applied differently between
    /// documents.
    ///
    /// The v1 document should have messages appended to each description.
    /// </summary>
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        /// <summary>
        /// Get a product
        /// </summary>
        /// <remarks>
        /// Gets a product.
        /// </remarks>
        /// <param name="id">The ID of the product.</param>
        /// <returns>The product.</returns>
        [HttpGet]
        public Product GetProduct([FromQuery] int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        /// <summary>
        /// Add a product
        /// </summary>
        /// <remarks>
        /// Adds a product.
        /// </remarks>
        /// <param name="product">The product to add.</param>
        /// <returns>The ID of the added product.</returns>
        [HttpPost]
        public int AddProduct([FromBody] Product product)
        {
            return product.Id == 0 ? 1 : product.Id;
        }
    }

    /// <summary>
    /// A product.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// The unique identifier for the product.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A description of the product.
        /// </summary>
        public string Description { get; set; }
    }
}