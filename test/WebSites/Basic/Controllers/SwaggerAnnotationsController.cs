using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Basic.Swagger;

namespace Basic.Controllers
{
    [SwaggerTag("Carts", "Manipulate Carts to your heart's content", "http://www.github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/master/README.md")]
    public class SwaggerAnnotationsController
    {
        [SwaggerOperation("CreateCart", Tags = new string[] { "Carts", "Checkout" })]
        [HttpPost("/carts")]
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
    }

    public class Cart
    {
        public int Id { get; internal set; }
    }
}