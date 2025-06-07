using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ReDoc.Controllers;

[Route("/products")]
[Produces("application/json")]
public class ProductsController
{
    [HttpPost]
    public int CreateProduct([FromBody, Required] Product product)
    {
        Debug.Assert(product is not null);
        return 1;
    }

    [HttpGet]
    public IEnumerable<Product> GetProducts()
    {
        return
        [
            new Product { Id = 1, Description = "A product" },
            new Product { Id = 2, Description = "Another product" },
        ];
    }

    [HttpGet("{id}")]
    public Product GetProduct(int id)
    {
        return new Product { Id = id, Description = "A product" };
    }

    [HttpPut("{id}")]
    public void UpdateProduct(int id, [FromBody, Required] Product product)
    {
        Debug.Assert(id >= 0);
        Debug.Assert(product is not null);
    }

    [HttpPatch("{id}")]
    public void PatchProduct(int id, [FromBody, Required] IDictionary<string, object> updates)
    {
        Debug.Assert(id >= 0);
        Debug.Assert(updates is not null);
    }

    [HttpDelete("{id}")]
    public void DeleteProduct(int id)
    {
        Debug.Assert(id >= 0);
    }
}

public class Product
{
    public int Id { get; set; }

    public string Description { get; set; }
}
