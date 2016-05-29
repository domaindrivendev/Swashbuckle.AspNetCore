using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using Basic.Swagger;

namespace Basic.Controllers
{
    public class SwaggerAnnotatedController
    {
        [SwaggerOperation("CreateCart")]
        [SwaggerResponseRemoveDefaults]
        [SwaggerResponse(201, "Cart created", typeof(Cart))]
        [HttpPost("/carts")]
        public Cart Create([FromBody]Cart cart)
        {
            return new Cart { Id = 1 };
        }

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
        public int Id { get; internal set; }
    }
}