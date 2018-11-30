using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using ApiKeySecurity.Models;

namespace ApiKeySecurity.Controllers
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

        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
