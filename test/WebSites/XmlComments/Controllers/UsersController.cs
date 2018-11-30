using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using XmlComments.Models;

namespace XmlComments.Controllers
{
    [ApiController]
    [Route("users")]
    [Consumes("application/json")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Creates a new user
        /// </summary>
        /// <remarks>
        /// Returns a unique id for the user
        /// </remarks>
        /// <param name="user">The user info</param>
        /// <response code="200">User created</response>
        /// <response code="400">Invalid user info</response>
        [HttpPost]
        public IActionResult CreateUser([FromBody, Required]User user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches existing users by keywords
        /// </summary>
        /// <param name="keywords">The keywords to search on</param>
        /// <param name="host">The server to which the request is being sent</param>
        /// <returns></returns>
        [HttpGet("search")]
        public IEnumerable<User> SearchUsers([FromQuery]string keywords, [FromQuery]PagingParams pagingParams, [FromHeader]string host)
        {
            throw new NotImplementedException();
        }
    }
}
