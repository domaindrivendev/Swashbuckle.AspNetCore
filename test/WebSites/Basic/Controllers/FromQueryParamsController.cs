using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("addresses/validate")]
        public IActionResult ValidateAddress([FromQuery]Address address)
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
        /// <summary>
        /// 3-letter ISO country code
        /// </summary>
        [Required]
        public string Country { get; set; }

        /// <summary>
        /// Name of city
        /// </summary>
        [DefaultValue("Seattle")]
        public string City { get; set; }
    }
}