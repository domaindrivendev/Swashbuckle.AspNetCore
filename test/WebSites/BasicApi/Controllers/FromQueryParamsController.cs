using Microsoft.AspNet.Mvc;

namespace BasicApi.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("adresses/valdidate")]
        public IActionResult ValidateAddress(Address address)
        {
            return new NoContentResult(); 
        }
    }

    public class Address
    {
        [FromQuery]
        public string Country { get; set; }

        [FromQuery]
        public string City { get; set; }
    }
}