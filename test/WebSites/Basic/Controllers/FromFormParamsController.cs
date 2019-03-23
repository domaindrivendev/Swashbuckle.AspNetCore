using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Basic.Controllers
{
    public class FromFormParamsController
    {
        [HttpPost("registrations")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult PostForm([FromForm]RegistrationForm form)
        {
            throw new System.NotImplementedException();
        }
    }

    public class RegistrationForm
    {
        public string Name { get; set; }

        public IEnumerable<int> PhoneNumbers { get; set; }
    }
}