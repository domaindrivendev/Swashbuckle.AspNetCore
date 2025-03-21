using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Basic.Controllers;

public class FromFormParamsController
{
    /// <summary>
    /// Form parameters with description
    /// </summary>
    /// <param name="form">Description for whole object</param>
    /// <param name="formFile">Description for file</param>
    /// <param name="text">Description for Text</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    [HttpPost("registrations")]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult PostForm([FromForm] RegistrationForm form, IFormFile formFile, [FromForm] string text)
    {
        throw new System.NotImplementedException();
    }

    [HttpPost("registrationsWithIgnoreProperties")]
    public IActionResult PostFormWithIgnoredProperties([FromForm] RegistrationFormWithIgnoredProperties form)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// Form parameters with description
    /// </summary>
    /// <param name="form">Description for whole object</param>
    /// <param name="formFile">Description for file</param>
    /// <param name="dateTimeKind">Description for dateTimeKind</param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    [HttpPost("registrationsWithEnumParameter")]
    public IActionResult PostFormWithEnumParameter([FromForm] RegistrationFormWithEnum form, IFormFile formFile, [FromForm] DateTimeKind dateTimeKind)
    {
        throw new System.NotImplementedException();
    }
}

public class RegistrationForm
{
    /// <summary>
    /// Summary for Name
    /// </summary>
    /// <example>MyName</example>
    public string Name { get; set; }

    /// <summary>
    /// Sumary for PhoneNumbers
    /// </summary>
    public IEnumerable<int> PhoneNumbers { get; set; }
}

public class RegistrationFormWithEnum
{
    /// <summary>
    /// Summary for Name
    /// </summary>
    /// <example>MyName</example>
    public string Name { get; set; }

    /// <summary>
    /// Sumary for PhoneNumbers
    /// </summary>
    public IEnumerable<int> PhoneNumbers { get; set; }

    /// <summary>
    /// Summary for LogLevel
    /// </summary>
    public LogLevel LogLevel { get; set; }
}

public class RegistrationFormWithIgnoredProperties
{
    [SwaggerIgnore, FromForm(Name = "internal_Name")]
    public string Name { get; set; }

    public IEnumerable<int> PhoneNumbers { get; set; }
}
