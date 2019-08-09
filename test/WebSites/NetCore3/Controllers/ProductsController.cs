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
    }

    public class Product
    {
        public int Id { get; internal set; }
    }
}
