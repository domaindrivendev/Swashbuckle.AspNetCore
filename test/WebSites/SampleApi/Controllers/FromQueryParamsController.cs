using Microsoft.AspNet.Mvc;
using System;

namespace SampleApi.Controllers
{
    [Produces("application/json")]
    public class FromQueryParamsController
    {
        [HttpGet("adresses/valdidate")]
        public bool ValidateAddress([FromQuery]Address address)
        {
            throw new NotImplementedException();
        }
    }

    public class Address
    {
        public string Country { get; set; }

        public string City { get; set; }
    }
}