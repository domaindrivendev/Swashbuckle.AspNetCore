using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OAuth2Integration.ResourceServer.Controllers
{
    [Route("products")]
    public class ProductsController : Controller
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "readAccess")]
        public IEnumerable<Product> GetAll()
        {
            yield return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Policy = "readAccess")]
        public Product GetById(int id)
        {
            return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };

        }

        [HttpPost]
        [Authorize("writeAccess")]
        public void Post([FromBody]Product product)
        {
        }

        [HttpDelete("{id}")]
        [Authorize("writeAccess")]
        public void Delete(int id)
        {
        }
    }

    public class Product
    {
        public int Id { get; internal set; }
        public string SerialNo { get; set; }
        public ProductStatus Status { get; set; }
    }

    public enum ProductStatus
    {
        InStock, ComingSoon 
    }
}