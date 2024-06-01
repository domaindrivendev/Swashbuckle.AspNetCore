using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Basic.Controllers
{
    public class FromFormParamsController
    {
        [HttpPost("registrations")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult PostForm([FromForm] RegistrationForm form)
        {
            throw new System.NotImplementedException();
        }

        [HttpPost("registrationsWithIgnoreProperties")]
        public IActionResult PostFormWithIgnoredProperties([FromForm] RegistrationFormWithIgnoredProperties form)
        {
            throw new System.NotImplementedException();
        }
    }

    public class RegistrationForm
    {
        public string Name { get; set; }

        public IEnumerable<int> PhoneNumbers { get; set; }
    }

    public class RegistrationFormWithIgnoredProperties
    {
        [SwaggerIgnore, FromForm(Name = "internal_Name")]
        public string Name { get; set; }

        public IEnumerable<int> PhoneNumbers { get; set; }
    }
}
