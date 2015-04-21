using System;
using Swashbuckle.Swagger.Annotations;
using Microsoft.AspNet.Mvc;

namespace SampleApi.Controllers
{
    public class SwaggerAnnotatedController
    {
        [SwaggerOperation("GetCartById")]
        [HttpGet("/carts/{id}")]
        public Cart GetById(int id)
        {
            throw new NotImplementedException();
        }
    }

    public class Cart
    {}
}