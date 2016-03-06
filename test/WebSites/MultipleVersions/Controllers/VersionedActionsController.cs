using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MultipleVersions.Versioning;

namespace MultipleVersions.Controllers
{
    [Route("/{version}/products")]
    public class VersionedActionsController
    {
        [HttpPost()]
        [Versions("v1", "v2")]
        public int Create([FromBody]Product product)
        {
            return 1;
        }

        [HttpGet()]
        [Versions("v1", "v2")]
        public IEnumerable<Product> GetAll()
        {
            return new[]
            {
                new Product { Id = 1, Description = "A product" },
                new Product { Id = 2, Description = "Another product" },
            };
        }

        [HttpGet("{id}")]
        [Versions("v1", "v2")]
        public Product GetById(int id)
        {
            return new Product { Id = id, Description = "A product" };
        }

        [HttpPut("{id}")]
        [Versions("v1", "v2")]
        public void Update(int id, [FromBody, Required]Product product)
        {
        }

        [HttpPatch("{id}")]
        [Versions("v2")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
        }

        [HttpDelete("{id}")]
        [Versions("v2")]
        public void Delete(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; set; }

        public string Description { get; set; }
    }
}