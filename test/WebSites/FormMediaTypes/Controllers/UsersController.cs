using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using FormMediaTypes.Models;

namespace FormMediaTypes.Controllers
{
    [ApiController]
    [Route("users")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public int CreateUser([FromForm, Required]User user)
        {
            throw new NotImplementedException();
        }
    }
}
