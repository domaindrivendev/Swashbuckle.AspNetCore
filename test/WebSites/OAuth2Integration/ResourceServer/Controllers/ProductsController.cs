using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace OAuth2Integration.ResourceServer.Controllers
{
    [Route("products")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductsController : Controller
    {
        [HttpGet]
        [Authorize("readAccess")]
        public IEnumerable<Product> GetProducts()
        {
            yield return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };
        }

        [HttpGet("{id}")]
        [Authorize("readAccess")]
        public Product GetProduct(int id)
        {
            return new Product
            {
                Id = 1,
                SerialNo = "ABC123",
            };

        }

        [HttpPost]
        [Authorize("writeAccess")]
        public void CreateProduct([FromBody]Product product)
        {
        }

        [HttpDelete("{id}")]
        [Authorize("writeAccess")]
        public void DeleteProduct(int id)
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