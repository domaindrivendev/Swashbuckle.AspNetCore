using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OAuth2Integration.ResourceServer.Controllers;

[Route("products")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ProductsController : Controller
{
    [HttpGet]
    [Authorize("readAccess")]
    public IEnumerable<Product> GetProducts()
    {
        yield return new Product
        {
            Id = 1,
            SerialNo = "ABC123",
        };
    }

    [HttpGet("{id}")]
    [Authorize("readAccess")]
    public Product GetProduct(int id)
    {
        Debug.Assert(id >= 0);
        return new Product
        {
            Id = 1,
            SerialNo = "ABC123",
        };

    }

    [HttpPost]
    [Authorize("writeAccess")]
    public void CreateProduct([FromBody] Product product)
    {
        Debug.Assert(product is not null);
    }

    [HttpDelete("{id}")]
    [Authorize("writeAccess")]
    public void DeleteProduct(int id)
    {
        Debug.Assert(id >= 0);
    }
}

public class Product
{
    public int Id { get; internal set; }

    public string SerialNo { get; set; }

    public ProductStatus Status { get; set; }
}

public enum ProductStatus
{
    InStock,
    ComingSoon,
}
