using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.SwaggerGen.Annotations;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("addresses/validate")]
        [SwaggerDescription("This method validates addresses", "Validates Addresses")]
        public IActionResult ValidateAddress([FromQuery]Address address, [FromQuery, SwaggerDescription("Test Boolean Description")]bool flag)
        {
            return new NoContentResult(); 
        }

        [HttpGet("zip-codes/validate")]
        public IActionResult ValidateZipCodes([FromQuery]IEnumerable<string> zipCodes)
        {
            return new NoContentResult(); 
        }
    }

    public class Address
    {
        [Required]
        [SwaggerDescription("3-letter ISO country code")]
        public string Country { get; set; }

        /// <summary>
        /// Name of city
        /// </summary>
        public string City { get; set; }
    }
}