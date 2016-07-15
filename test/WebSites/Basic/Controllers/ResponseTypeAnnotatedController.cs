using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("/orders")]
    [Produces("application/json")]
    public class ResponseTypeAnnotatedController
    {
        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 400)]
        public IActionResult Create([FromBody, Required]Order order)
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