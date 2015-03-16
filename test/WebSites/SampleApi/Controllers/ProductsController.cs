using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using SampleApi.Models;

namespace SampleApi.Controllers
{
    public class ProductsController
    {
        [HttpGet("/products")]
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}