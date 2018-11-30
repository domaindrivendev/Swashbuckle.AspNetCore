using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FormMediaTypes.Models;

namespace FormMediaTypes.Controllers
{
    [ApiController]
    [Route("products")]
    public class ProductsController : ControllerBase
    {
        [HttpPost]
        public int CreateProduct([FromForm]Product product)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}/picture")]
        public void UpdatePicture(IFormFile picture)
        {
        }

        [HttpPut("{id}/gallery")]
        public void UpdateGallery(IFormFileCollection gallery)
        {
        }
    }
}
