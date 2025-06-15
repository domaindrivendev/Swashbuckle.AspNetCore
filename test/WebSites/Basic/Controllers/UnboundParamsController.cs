using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Route("/stores")]
[Produces("application/json")]
public class UnboundParamsController
{

    [HttpPost]
    public int Create(Store store)
    {
        Debug.Assert(store is not null);
        return 1;
    }

    [HttpGet]
    public IEnumerable<Store> Search(string[] locations = null)
    {
        Debug.Assert(locations is not null);
        return
        [
            new Store { Id = 1, Location = "Boston" },
            new Store { Id = 1, Location = "Seattle" }
        ];
    }

    [HttpGet("{id}")]
    public Store GetById(int id)
    {
        Debug.Assert(id >= 0);
        return new Store { Id = 1, Location = "Boston" };
    }

    [HttpPut("{id}")]
    public void Update(int id, Store store)
    {
        Debug.Assert(id >= 0);
        Debug.Assert(store is not null);
    }

    [HttpDelete("{id}")]
    public void Delete(int id)
    {
        Debug.Assert(id >= 0);
    }
}

public class Store
{
    public int Id { get; set; }

    public string Location { get; set; }
}
