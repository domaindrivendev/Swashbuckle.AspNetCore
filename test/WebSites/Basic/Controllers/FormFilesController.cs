using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("files")]
    public class FormFilesController
    {
        [HttpPost("single")]
        public IActionResult PostFile(IFormFile file)
        {
            throw new NotImplementedException();
        }

        [HttpPost("multiple")]
        public IActionResult PostFiles(IFormFileCollection files)
        {
            throw new NotImplementedException();
        }

        [HttpPost("form-with-file")]
        public IActionResult PostFormWithFile([FromForm]FormWithFile formWithFile)
        {
            throw new NotImplementedException();
        }
    }

    public class FormWithFile
    {
        public string Name { get; set; }

        public IFormFile File { get; set; }
    }
}