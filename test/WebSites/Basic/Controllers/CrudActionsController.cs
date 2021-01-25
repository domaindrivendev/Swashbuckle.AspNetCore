using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    /// <summary>
    /// Summary for CrudActionsController
    /// </summary>
    [Route("/products")]
    [Produces("application/json")]
    public class CrudActionsController
    {
        /// <summary>
        /// Creates a <paramref name="product"/>
        /// </summary>
        /// <remarks>
        /// ## Heading 1
        /// 
        ///     POST /products
        ///     {
        ///         "id": "123",
        ///         "description": "Some product"
        ///     }
        /// 
        /// </remarks>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost(Name = "CreateProduct")]
        public Product Create([FromBody, Required]Product product)
        {
            return product;
        }

        /// <summary>
        /// Searches the collection of products by description key words
        /// </summary>
        /// <param name="keywords" example="hello">A list of search terms</param>
        /// <returns></returns>
        [HttpGet(Name = "SearchProducts")]
        public IEnumerable<Product> Get([FromQuery(Name = "kw")]string keywords = "foobar")
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }

        /// <summary>
        /// Returns a specific product
        /// </summary>
        /// <param name="id" example="111">The product id</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetProduct")]
        public Product Get(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        /// <summary>
        /// Updates all properties of a specific product
        /// </summary>
        /// <param name="id" example="222"></param>
        /// <param name="product"></param>
        [HttpPut("{id}", Name = "UpdateProduct")]
        public void Update(int id, [FromBody, Required]Product product)
        {
        }

        /// <summary>
        /// Updates some properties of a specific product
        /// </summary>
        /// <param name="id" example="333"></param>
        /// <param name="updates"></param>
        [HttpPatch("{id}", Name = "PatchProduct")]
        public void Patch(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="id" example="444"></param>
        [HttpDelete("{id}", Name = "DeleteProduct")]
        public void Delete(int id)
        {
        }
    }

    public enum ProductStatus
    {
        All = 0,
        OutOfStock = 1,
        InStock = 2
    }

    /// <summary>
    /// Represents a product
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Uniquely identifies the product
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Describes the product
        /// </summary>
        public string Description { get; set; }

        public ProductStatus Status { get; set; }

        public ProductStatus? Status2 { get; set; }
    }
}