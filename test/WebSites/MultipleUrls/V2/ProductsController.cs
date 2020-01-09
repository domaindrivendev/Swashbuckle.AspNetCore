using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MultipleUrls.V1;

namespace MultipleUrls.V2
{
    [ApiVersion("2.0")]
    [ApiController]
    [Route("[controller]")]
    public class ProductsController
    {
        [HttpPost]
        public int CreateProduct([FromBody, Required]Product product)
        {
            return 1;
        }

        [HttpPut("{id}")]
        public void UpdateProduct(int id, [FromBody, Required]Product product)
        {
        }

        [HttpDelete("{id}")]
        public void DeleteProduct(int id)
        {
        }
    }
}