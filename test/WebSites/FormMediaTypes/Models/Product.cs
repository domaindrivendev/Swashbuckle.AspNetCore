using Microsoft.AspNetCore.Http;

namespace FormMediaTypes.Models
{
    public class Product
    {
        public int Id { get; }
        public string Description { get; set; }
        public IFormFile Picture { get; set; }
        public IFormFileCollection Gallery { get; set; }
    }
}