using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MultipleDocuments.Admin.Models;

namespace MultipleDocuments.Admin.Controllers
{
    [ApiController]
    [Route("admin/users")]
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
