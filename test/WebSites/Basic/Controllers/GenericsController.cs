using System;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    /// <summary>
    /// Summary for GenericsController
    /// </summary>
    [Route("/generics")]
    [Produces("application/json")]
    public class GenericsController
    {
        [HttpPost(Name = "Createstring")]
        public string Create([FromBody] GenericType<string> genericString)
        {
            throw new NotImplementedException();
        }
    }
}
