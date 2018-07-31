using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace CliExampleWithFactory.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        [HttpGet]
        public IEnumerable<Product> GetAll()
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }
}