using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SystemTextJsonSupport.Models;

namespace SystemTextJsonSupport.Controllers
{
    [ApiController]
    [Route("users")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public int CreateUser([FromBody, Required]User user)
        {
            throw new NotImplementedException();
        }
    }
}
