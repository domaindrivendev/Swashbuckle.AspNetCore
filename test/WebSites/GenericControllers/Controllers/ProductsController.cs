using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers
{
    [Route("products")]
    public class ProductsController : GenericResourceController<Product>
    { }

    public class Product
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

}
