using Microsoft.AspNet.Mvc;

namespace BasicApi.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("adresses/valdidate")]
        public IActionResult ValidateAddress([FromQuery]Address address)
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