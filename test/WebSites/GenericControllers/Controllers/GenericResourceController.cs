using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers
{
    public abstract class GenericResourceController<TResource> where TResource : new()
    {
        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [Consumes("application/json")]
        public int Create([FromBody, Required]TResource resource)
        {
            return 1;
        }

        ///// <summary>
        ///// Retrieves all resources
        ///// </summary>
        //[HttpGet]
        //[Produces("application/json")]
        //public IEnumerable<TResource> GetAll(string keywords)
        //{
        //    return new[] { new TResource(), new TResource() };
        //}

        ///// <summary>
        ///// Retrieves a specific resource
        ///// </summary>
        //[HttpGet("{id}")]
        //[Produces("application/json")]
        //public TResource GetById(int id)
        //{
        //    return new TResource();
        //}

        //[HttpPut("{id}")]
        //[Consumes("application/json")]
        //public void Update(int id, [FromBody, Required]TResource resource)
        //{
        //}

        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        //[HttpPut("{id}/files")]
        //[Consumes("multipart/form-data")]
        //public void UploadFile(int id, IFormFile files)
        //{
        //}
    }
}