using Microsoft.AspNetCore.Mvc;

namespace NetCore3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        [HttpPost(Name = "CreateProduct")]
        public ActionResult<int> Post(Product product)
        {
            return 123;
        }

        [HttpGet("{id}", Name = "GetProductById")]
        public ActionResult<Product> Get(int id)
        {
            return new Product
            {
                Id = id,
                Status = ProductStatus.InStock
            };
        }
    }

    public class Product
    {
        public int Id { get; internal set; }

        public ProductStatus Status { get; set; }
    }

    public enum ProductStatus
    {
        All = 0,
        OutOfStock = 1,
        InStock = 2
    }
}
