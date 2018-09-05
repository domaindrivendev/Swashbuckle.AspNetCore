using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ReDoc.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        [HttpPost]
        public int CreateProduct([FromBody, Required]Product product)
        {
            return 1;
        }

        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }

        [HttpGet("{id}")]
        public Product GetProduct(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        [HttpPut("{id}")]
        public void UpdateProduct(int id, [FromBody, Required]Product product)
        {
        }

        [HttpPatch("{id}")]
        public void PatchProduct(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }
}