using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MultipleVersions.V1;

namespace MultipleVersions.V2;

[ApiVersion("2.0")]
[ApiController]
[Route("[controller]")]
public class ProductsController
{
    [HttpPost]
    public int CreateProduct([FromBody, Required] Product product)
    {
        Debug.Assert(product is not null);
        return 1;
    }

    [HttpPut("{id}")]
    public void UpdateProduct(int id, [FromBody, Required] Product product)
    {
        Debug.Assert(product is not null);
    }

    [HttpDelete("{id}")]
    public void DeleteProduct(int id)
    {
        Debug.Assert(id >= 0);
    }
}
