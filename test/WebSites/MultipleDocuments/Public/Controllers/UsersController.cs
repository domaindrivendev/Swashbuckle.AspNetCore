using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MultipleDocuments.Public.Models;

namespace MultipleDocuments.Public.Controllers
{
    [ApiController]
    [Route("public/users")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<User> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}
