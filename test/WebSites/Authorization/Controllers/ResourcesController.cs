using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.Controllers;

[Route("resources")]
public class ResourcesController : Controller
{
    [HttpGet]
    public IEnumerable<Resource> GetAll()
    {
        return [new Resource { Id = 1, Name = "Public Resource" }];
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "readAccess")]
    public Resource GetById(int id)
    {
        return new Resource { Id = id, Name = "Protected Resource" };
    }

    [HttpPost]
    [Authorize(Policy = "writeAccess")]
    public Resource Create([FromBody] Resource resource)
    {
        return resource;
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "writeAccess")]
    public void Delete(int id)
    {
    }
}

public class Resource
{
    public int Id { get; set; }

    public string Name { get; set; }
}
