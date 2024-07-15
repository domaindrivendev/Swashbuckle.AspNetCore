﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers
{
    public abstract class GenericResourceController<TResource> where TResource : new()
    {
        /// <summary>
        /// Creates a resource
        /// </summary>
        /// <param name="resource">The resource</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [Consumes("application/json")]
        public int Create([FromBody, Required] TResource resource, CancellationToken cancellationToken)
        {
            return 1;
        }

        /// <summary>
        /// Delete by Id
        /// </summary>
        /// <param name="id">deleting Id</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <response code="200">Deleted</response>
        /// <response code="404">Failed</response>
        [HttpDelete($"{nameof(Delete)}ById")]
        public virtual int Delete([Required, FromBody] TResource id, CancellationToken cancellationToken)
        {
            return 1;
        }

        /// <summary>
        /// Delete by Id List
        /// </summary>
        /// <param name="ids">deleting Ids</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <response code="200">Deleted</response>
        /// <response code="404">Failed</response>
        [HttpDelete($"{nameof(Delete)}/List")]
        public virtual int Delete([Required, FromBody] List<TResource> ids, CancellationToken cancellationToken)
        {
            return 1;
        }

        /// <summary>
        /// Delete by Ids
        /// </summary>
        /// <param name="resources">deleting Ids</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <response code="200">Deleted</response>
        /// <response code="404">Failed</response>
        [HttpDelete("")]
        public virtual int Delete([Required, FromBody] TResource[] resources, CancellationToken cancellationToken)
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
