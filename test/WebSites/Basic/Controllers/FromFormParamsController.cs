using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Basic.Controllers
{
    public class FromFormParamsController
    {
        [HttpPost("registrations")]
        [Consumes("multipart/form-data")]
        public IActionResult PostForm([FromForm]RegistrationForm form)
        {
            throw new System.NotImplementedException();
        }
    }

    public class RegistrationForm
    {
        //[FromForm]
        public string Name { get; set; }

        //[FromForm]
        public IEnumerable<int> PhoneNumbers { get; set; }
    }
}