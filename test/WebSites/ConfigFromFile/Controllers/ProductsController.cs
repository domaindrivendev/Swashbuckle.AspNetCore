using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ConfigFromFile.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<Product> GetProducts([FromQuery]QueryParams queryParams)
        {
            throw new NotImplementedException();
        }
    }

    public class QueryParams
    {
        [Required]
        public string Foo { get; set; }

        public int Bar { get; set; }
    }

    public class Product
    {
        [JsonIgnore]
        public int Id { get; set; }

        public int[][] Foo { get; set; }
    }
}
