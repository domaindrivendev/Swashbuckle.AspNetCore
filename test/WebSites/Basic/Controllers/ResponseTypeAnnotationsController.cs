using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("/orders")]
    [Consumes("application/xml")]
    [Produces("application/xml")]
    public class ResponseTypeAnnotationsController
    {
        /// <summary>
        /// Creates an order 
        /// </summary>
        /// <param name="order"></param>
        /// <response code="201">Order created</response>
        /// <response code="400">Order invalid</response>
        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public IActionResult CreateOrder([FromBody, Required]Order order)
        {
            return new CreatedResult("/orders/1", 1);
        }
    }

    public class Order
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public decimal Total { get; set; }
    }
}