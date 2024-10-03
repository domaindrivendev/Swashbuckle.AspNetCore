using Microsoft.AspNetCore.Http.HttpResults;
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
    public IActionResult Get(LogLevel? logLevel = LogLevel.Error) => Ok(new { logLevel });

    [HttpGet("WithResults")]
    public Results<Ok<LogLevel?>, NotFound> GetWithResults(LogLevel? logLevel = LogLevel.Error) => TypedResults.Ok(logLevel);

    [HttpGet("WithTaskResults")]
    public async Task<Results<Ok<LogLevel?>, NotFound>> GetTaskWithResults(LogLevel? logLevel = LogLevel.Error)
    {
        await Task.Delay(1);
        return TypedResults.Ok(logLevel);
    }

    [HttpGet("WithValueTaskResults")]
    public async ValueTask<Results<Ok<LogLevel?>, NotFound>> GetValueTaskWithResults(LogLevel? logLevel = LogLevel.Error)
    {
        await Task.Delay(1);
        return TypedResults.Ok(logLevel);
    }
}

namespace MvcWithNullable
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
