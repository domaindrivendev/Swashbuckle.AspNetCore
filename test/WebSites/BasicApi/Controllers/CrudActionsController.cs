using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BasicApi.Controllers
{
    [Route("/products")]
    [Produces("application/json")]
    public class CrudActionsController
    {
        [HttpPost()]
        public int Create([FromBody, Required]Product product)
        {
            throw new NotImplementedException();
        }

        [HttpGet()]
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }

        //[HttpGet("/products")]
        //public IEnumerable<Product> GetAllByType([FromQuery, Required]ProductType type)
        //{
        //    throw new NotImplementedException();
        //}

        [HttpGet("{id}")]
        public Product GetById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        public void Update(int id, [FromBody, Required]Product product)
        {
            throw new NotImplementedException();
        }

        [HttpPatch("{id}")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }

    public enum ProductType
    {
        Album,
        Book
    }
}