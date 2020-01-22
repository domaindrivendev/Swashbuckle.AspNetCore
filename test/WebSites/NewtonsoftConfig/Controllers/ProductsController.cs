using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NewtonsoftConfig
{
    [Route("/products")]
    [Produces("application/json")]
    public class ProductsController : Controller
    {
        [HttpGet]
        public IEnumerable<Product> GetProducts([FromQuery]ProductStatus status)
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
        /// <summary>
        /// Product identifier
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int Id { get; set; }

        /// <summary>
        /// Product description
        /// </summary>
        [JsonProperty("my-description")]
        public string Description { get; set; }

        /// <summary>
        /// Product status
        /// </summary>
        [JsonProperty]
        public ProductStatus Status { get; set; }

        /// <summary>
        /// Product registration date
        /// </summary>
        public DateTime RegisteredOn { get; set; }

        /// <summary>
        /// Product reference
        /// </summary>
        [System.Runtime.Serialization.DataMember(Name = "ref")]
        public string Reference { get; set; }
    }

    public enum ProductStatus
    {
        All = 0,
        OutOfStock = 1,
        InStock = 2
    }
}