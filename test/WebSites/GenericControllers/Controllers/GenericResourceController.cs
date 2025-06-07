using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GenericControllers.Controllers;

public abstract class GenericResourceController<TResource>
    where TResource : new()
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
    public int Create(
        [FromBody, Required] TResource resource,
        CancellationToken cancellationToken)
    {
        Debug.Assert(resource is not null);
        Debug.Assert(cancellationToken.CanBeCanceled);
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
    [HttpDelete("DeleteById")]
    public virtual int Delete(
        [Required, FromBody] TResource id,
        CancellationToken cancellationToken)
    {
        Debug.Assert(id is not null);
        Debug.Assert(cancellationToken.CanBeCanceled);
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
    [HttpDelete("Delete/List")]
    public virtual int Delete(
        [Required, FromBody] List<TResource> ids,
        CancellationToken cancellationToken)
    {
        Debug.Assert(ids is not null);
        Debug.Assert(cancellationToken.CanBeCanceled);
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
    public virtual int Delete(
        [Required, FromBody] TResource[] resources,
        CancellationToken cancellationToken)
    {
        Debug.Assert(resources is not null);
        Debug.Assert(cancellationToken.CanBeCanceled);
        return 1;
    }
}
