using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace NetCore21
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        [HttpGet]
        public IEnumerable<Product> GetProducts()
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

        public ProductStatus Status { get; set; }
    }

    public enum ProductStatus
    {
        All = 0,
        OutOfStock = 1,
        InStock = 2
    }
}