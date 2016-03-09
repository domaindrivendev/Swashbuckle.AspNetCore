using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("adresses/valdidate")]
        public IActionResult ValidateAddress([FromQuery]Address addresse)
        {
            return new NoContentResult(); 
        }

        [HttpGet("zip-codes/valdidate")]
        public IActionResult ValidateZipCodes([FromQuery]IEnumerable<string> zipCodes)
        {
            return new NoContentResult(); 
        }
    }

    public class Address
    {
        public string Country { get; set; }

        public string City { get; set; }
    }
}