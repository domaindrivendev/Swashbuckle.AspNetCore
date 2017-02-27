using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using Basic.Swagger;

namespace Basic.Controllers
{
    public class SwaggerAnnotationsController
    {
        [SwaggerOperation("CreateCart")]
        [SwaggerOperationFilter(typeof(AddCreateCartResponseExamples))]
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

    [SwaggerSchemaFilter(typeof(AddCartDefault))]
    public class Cart
    {
        public int Id { get; internal set; }
    }
}