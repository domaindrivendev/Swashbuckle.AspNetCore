using Microsoft.AspNet.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using Basic.Swagger;

namespace Basic.Controllers
{
    public class SwaggerAnnotatedController
    {
        [SwaggerOperation("GetCartById")]
        [SwaggerResponse(404, "Cart not found")]
        [HttpGet("/carts/{id}")]
        public Cart GetById(int id)
        {
            return new Cart { Id = id };
        }
    }

    [SwaggerModelFilter(typeof(AddCartDefault))]
    public class Cart
    {
        public int Id { get; set; }
    }
}