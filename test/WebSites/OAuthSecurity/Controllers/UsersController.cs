using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthSecurity.Models;

namespace OAuthSecurity.Controllers
{
    [ApiController]
    [Route("users")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        [Authorize("writeAccess")]
        public int CreateUser([FromBody, Required]User user)
        {
            throw new NotImplementedException();
        }

        [HttpGet]
        [Authorize("readAccess")]
        public IEnumerable<User> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
