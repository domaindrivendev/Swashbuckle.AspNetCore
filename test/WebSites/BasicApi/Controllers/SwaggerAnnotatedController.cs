using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Mvc;
using BasicApi.Swagger;

namespace BasicApi.Controllers
{
    public class SwaggerAnnotatedController
    {
        [SwaggerOperation("GetCartById")]
        [SwaggerResponse(404, "Cart not found")]
        [SwaggerPathDetails("Gets the cart by Id",
            "Each cart is identified by a unique Id, you can obtain the details of the cart by supplying this id. However, the cart Id may change once emptied so you need to check from the user account, the current cart Id")]
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