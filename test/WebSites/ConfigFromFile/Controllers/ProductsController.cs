using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigFromFile.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<Product> GetProducts()
        {
            throw new NotImplementedException();
        }
    }

    public class Product
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int[][] Foo { get; set; }
    }
}
