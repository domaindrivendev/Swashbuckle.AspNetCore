using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;

namespace Basic.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class CrudActionsController
    {
        /// <summary>
        /// Creates a product
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        [HttpPost]
        public int Create([FromBody, Required]Product product)
        {
            return 1;
        }

        /// <summary>
        /// Returns the collection of all products
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Product> GetAll()
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
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public Product GetById(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        /// <summary>
        /// Updates all properties of a specific product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="product"></param>
        [HttpPut("{id}")]
        public void Update(int id, [FromBody, Required]Product product)
        {
        }

        /// <summary>
        /// Updates some properties of a specific product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updates"></param>
        [HttpPatch("{id}")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        /// <summary>
        /// Deletes a specific product
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
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
    }
}