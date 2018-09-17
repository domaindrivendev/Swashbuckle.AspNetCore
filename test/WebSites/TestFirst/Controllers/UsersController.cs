using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TestFirst.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        [HttpPost]
        public IActionResult CreateUser([FromBody]CreateUserBody createUserBody)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            return Created("/api/users/1", null);
        }
    }

    public class CreateUserBody
    {
        [Required]
        public string Email { get; set; } 
        [Required]
        public string Password { get; set; }

    }
}
