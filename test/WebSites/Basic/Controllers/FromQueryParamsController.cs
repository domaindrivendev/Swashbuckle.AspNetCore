using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Basic.Controllers;

[Produces("application/json")]
public class FromQueryParamsController
{
    [HttpGet("addresses/validate")]
    public IActionResult ValidateAddress([FromQuery] Address address)
    {
        Debug.Assert(address is not null);
        return new NoContentResult(); 
    }

    [HttpGet("zip-codes/validate")]
    public IActionResult ValidateZipCodes(
        [FromQuery]IEnumerable<string> zipCodes,
        [FromQuery(Name = "search")] [Required] Dictionary<string, string> parameters)
    {
        Debug.Assert(zipCodes is not null);
        Debug.Assert(parameters is not null);
        return new NoContentResult(); 
    }
}

public class Address
{
    /// <summary>
    /// 3-letter ISO country code
    /// </summary>
    [Required]
    public string Country { get; set; }

    /// <summary>
    /// Name of city
    /// </summary>
    [DefaultValue("Seattle")]
    public string City { get; set; }
}
