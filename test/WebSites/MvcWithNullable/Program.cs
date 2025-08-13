using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
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
    [HttpGet]
    public IActionResult Get(LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });
}

[ApiController]
[Route("api/[controller]")]
public class RequiredEnumController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([Required] LogLevel? logLevel = LogLevel.Error)
        => Ok(new { logLevel });
}
