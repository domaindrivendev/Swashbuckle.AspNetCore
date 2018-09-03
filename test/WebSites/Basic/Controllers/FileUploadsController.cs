using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    public class FileUploadsController
    {
        [HttpPost("file")]
        public IActionResult PostFile(IFormFile file)
        {
            throw new NotImplementedException();
        }

        [HttpPost("form-with-file")]
        public IActionResult PostFormWithFile(FormWithFile formWithFile)
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