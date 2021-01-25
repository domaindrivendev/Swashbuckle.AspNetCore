using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Basic.Swagger;

namespace Basic.Controllers
{
    [SwaggerTag("Manipulate Carts to your heart's content", "http://www.tempuri.org")]
    public class SwaggerAnnotationsController
    {
        [HttpPost("/carts")]
        [SwaggerOperation(OperationId = "CreateCart")]
        [SwaggerResponse(201, "The cart was created", typeof(Cart))]
        [SwaggerResponse(400, "The cart data is invalid")]
        public Cart Create([FromBody, SwaggerRequestBody(Description = "The cart request body")]Cart cart)
        {
            return new Cart { Id = 1 };
        }

        [HttpGet("/carts/{id}")]
        [SwaggerOperation(OperationId = "GetCart")]
        [SwaggerOperationFilter(typeof(AddCartsByIdGetExternalDocs))]
        public Cart Get([SwaggerParameter("The cart identifier")]int id)
        {
            return new Cart { Id = id };
        }

        [HttpDelete("/carts/{id}")]
        [SwaggerOperation(
            OperationId = "DeleteCart",
            Summary = "Deletes a specific cart",
            Description = "Requires admin privileges")]
        public Cart Delete([FromRoute(Name = "id"), SwaggerParameter("The cart identifier")]int cartId)
        {
            return new Cart { Id = cartId };
        }
    }

    [SwaggerSchema(Required = new[] { "Id" })]
    public class Cart
    {
        [SwaggerSchema("The cart identifier", ReadOnly = true)]
        public int Id { get; set; }

        public CartType CartType { get; set; }
    }

    [SwaggerSchema(Description = "The cart type")]
    public enum CartType
    {
        Anonymous,
        Authenticated
    }
}