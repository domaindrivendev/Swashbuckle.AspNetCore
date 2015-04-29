using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Mvc;

namespace BasicApi.Controllers
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

    public class Cart
    {
        public int Id { get; set; }
    }
}