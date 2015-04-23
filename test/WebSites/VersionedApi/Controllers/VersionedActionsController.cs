using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json.Linq;
using VersionedApi.Versioning;

namespace VersionedApi.Controllers
{
    [Route("/{version}/products")]
    [Produces("application/json")]
    public class VersionedActionsController
    {
        [HttpPost()]
        [Versions("v1", "v2")]
        public int Create([FromBody]JObject product)
        {
            throw new NotImplementedException();
        }

        [HttpGet()]
        [Versions("v1", "v2")]
        public IEnumerable<JObject> GetAll()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        [Versions("v1", "v2")]
        public dynamic GetById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpPut("{id}")]
        [Versions("v1", "v2")]
        public void Update(int id, [FromBody, Required]JObject product)
        {
            throw new NotImplementedException();
        }

        [HttpPatch("{id}")]
        [Versions("v2")]
        public void PartialUpdate(int id, [FromBody, Required]IDictionary<string, object> updates)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{id}")]
        [Versions("v2")]
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}