using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreLatest.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        [HttpPost]
        public int Create([FromBody, Required]Product product)
        {
            return 1;
        }

        [HttpGet]
        public IEnumerable<Product> GetAll()
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }

        [HttpGet("{id}")]
        public Product GetById(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        [HttpPut("{id}")]
        public void Update(int id, [FromBody, Required]Product product)
        {
        }

        [HttpPatch("{id}")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }
}