using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using XmlMediaTypes.Models;

namespace XmlMediaTypes.Controllers
{
    [ApiController]
    [Route("users")]
    [Consumes("application/xml")]
    [Produces("application/xml")]
    public class UsersController : ControllerBase
    {
        [HttpPost]
        public int CreateUser([FromBody, Required]User user)
        {
            throw new NotImplementedException();
        }
    }
}
