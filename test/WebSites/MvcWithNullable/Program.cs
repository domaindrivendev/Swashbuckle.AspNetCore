using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.UseOneOfForNullableEnums();
    options.UseAllOfToExtendReferenceSchemas();
});

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

[ApiController]
[Route("api/[controller]")]
public class EnumController : ControllerBase
{
    [HttpGet("query")]
    public IActionResult GetQuery(LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });

    [HttpGet("path/{logLevel}")]
    public IActionResult GetPath(LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });

    [HttpGet("header")]
    public IActionResult GetHeader([FromHeader] LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });

    [HttpGet("enum-body")]
    public IActionResult GetEnumBody([FromBody] LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });

    [HttpGet("type-body")]
    public IActionResult GetTypeBody([FromBody] TypeWithNullable typeWithNullable)
        => Ok(new { typeWithNullable.LogLevel });
}

[ApiController]
[Route("api/[controller]")]
public class RequiredEnumController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromBody, Required] LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });
}

public class TypeWithNullable
{
    public LogLevel? LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;

    [Required]
    public LogLevel? RequiredLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;

    [JsonRequired]
    public LogLevel? JsonRequiredLogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Error;
}

namespace MvcWithNullable
{
    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>
    /// </summary>
    public partial class Program
    {
    }
}
