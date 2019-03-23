using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("files")]
    public class FilesController : Controller
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

        [HttpGet("{name}")]
        [Produces("application/octet-stream", Type = typeof(FileResult))]
        public FileResult GetFile(string name)
        {
            var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            writer.WriteLine("Hello world!");
            writer.Flush();
            stream.Position = 0;

            return File(stream, "application/octet-stream", name);
        }
    }

    public class FormWithFile
    {
        public string Name { get; set; }

        public IFormFile File { get; set; }
    }
}