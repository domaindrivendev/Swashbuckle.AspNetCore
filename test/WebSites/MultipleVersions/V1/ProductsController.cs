﻿using Microsoft.AspNetCore.Mvc;

namespace MultipleVersions.V1
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [Route("[controller]")]
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
    }
}