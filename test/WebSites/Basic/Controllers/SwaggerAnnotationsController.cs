using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;
using Basic.Swagger;
using System.Collections.Generic;

namespace Basic.Controllers
{
    public class SwaggerAnnotationsController
    {
        [SwaggerOperation("CreateCart")]
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

    [SwaggerModelFilter(typeof(AddCartDefault))]
    public class Cart
    {
        public int Id { get; internal set; }
    }
}