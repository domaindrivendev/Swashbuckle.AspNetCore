using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers
{
    [Route("/stores")]
    [Produces("application/json")]
    public class UnboundParamsController
    {

        [HttpPost]
        public int Create(Store store)
        {
            return 1;
        }

        [HttpGet]
        public IEnumerable<Store> Search(string[] locations = null)
        {
            return new[]
            {
                new Store { Id = 1, Location = "Boston" },
                new Store { Id = 1, Location = "Seattle" }
            };
        }

        [HttpGet("{id}")]
        public Store GetById(int id)
        {
            return new Store { Id = 1, Location = "Boston" };
        }

        [HttpPut("{id}")]
        public void Update(int id, Store store)
        {
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }

    public class Store
    {
        public int Id { get; set; }

        public string Location { get; set; }
    }
}