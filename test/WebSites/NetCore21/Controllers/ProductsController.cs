using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;

namespace NetCore21
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController
    {
        [HttpGet]
        public IEnumerable<Product> GetProducts([FromQuery]ProductType type = ProductType.Digital)
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product", Type = ProductType.Physical },
                new Product { Id = 2, Description = "Another product", Type = ProductType.Digital },
            };
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }

        [DefaultValue(ProductType.Digital)]
        public ProductType Type { get; set; }
    }

    public enum ProductType
    {
        Physical = 0,
        Digital = 1,
    }
}