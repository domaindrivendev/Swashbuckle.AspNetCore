using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Basic.Swagger;

namespace Basic.Controllers
{
    [SwaggerTag("Manipulate Carts to your heart's content", "http://www.tempuri.org")]
    public class SwaggerAnnotationsController
    {
        [HttpPost("/carts")]
        [SwaggerOperation("CreateCart", Tags = new string[] { "Carts", "Checkout" })]
        public Cart Create([FromBody]Cart cart)
        {
            return new Cart { Id = 1 };
        }

        [HttpGet("/carts/{id}")]
        [SwaggerOperationFilter(typeof(AddCartsByIdGetExternalDocs))]
        public Cart GetById(int id)
        {
            return new Cart { Id = id };
        }

        [HttpDelete("/carts/{id}")]
        [SwaggerOperation(
            Consumes = new string[] { "test/plain", "application/json" },
            Produces = new string[] { "application/javascript", "application/xml" })]
        public Cart Delete([FromRoute(Name = "id"), SwaggerParameter("The cart identifier")]int cartId)
        {
            return new Cart { Id = cartId };
        }
    }

    public class Cart
    {
        public int Id { get; internal set; }
    }
}