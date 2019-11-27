using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class DataAnnotationsController : Controller
    {
        [HttpPost("payments/authorize")]
        [ProducesResponseType(200, Type = typeof(string))]
        public IActionResult AuthorizePayment([FromBody, Required]PaymentRequest request)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            return new ObjectResult("123456");
        }

        [HttpPut("payments/{paymentId}/cancel")]
        public IActionResult CancelPayment([MinLength(6)]string paymentId)
        {
            return Ok();
        }
    }

    public class PaymentRequest
    {
        [Required]
        public Transaction Transaction { get; set; }

        [Required]
        public CreditCard CreditCard { get; set; }
    }

    public class Transaction
    {
        [Required]
        public decimal Amount { get; set; }

        public string Note { get; set; }
    }

    public class CreditCard
    {
        [Required, RegularExpression("^[3-6]?\\d{12,15}$")]
        public string CardNumber { get; set; }

        [Required, Range(1, 12)]
        public int ExpMonth { get; set; }

        [Required, Range(14, 99)]
        public int ExpYear { get; set; }
    }
}