using System;
using Microsoft.AspNetCore.Mvc;
using ListingResponseTypes.Models;

namespace ListingResponseTypes.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("products")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        [HttpPost]
        public ActionResult<int> CreateProduct([FromBody]Product product)
        {
            throw new NotImplementedException();
        }
    }
}
