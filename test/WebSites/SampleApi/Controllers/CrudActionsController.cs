using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SampleApi.Controllers
{
    public class CrudActionsController
    {
        [HttpPost("/products")]
        public int Create([FromBody, Required]Product product)
        {
            throw new NotImplementedException();
        }

        [HttpGet("/products")]
        public IEnumerable<Product> GetAll()
        {
            throw new NotImplementedException();
        }

        [HttpGet("/products")]
        public IEnumerable<Product> GetAllByType([FromQuery, Required]ProductType type)
        {
            throw new NotImplementedException();
        }

        [HttpGet("/products/{id}")]
        public Product GetById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("/products/{id}")]
        public void Update(int id, [FromBody, Required]Product product)
        {
            throw new NotImplementedException();
        }

        [HttpPatch("/products/{id}")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("/products/{id}")]
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