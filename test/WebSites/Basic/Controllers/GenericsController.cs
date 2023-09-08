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
        [HttpPost(Name = "CreateString")]
        public string CreateString([FromBody] GenericType<string> genericString)
        {
            throw new NotImplementedException();
        }

        [HttpPost(Name = "CreateDateTime")]
        public string CreateDateTime([FromBody] GenericType<DateTime> genericObject)
        {
            throw new NotImplementedException();
        }
    }
}
