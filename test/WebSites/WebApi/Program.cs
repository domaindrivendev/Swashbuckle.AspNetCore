using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new() { Title = "WebApi", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/fruit/{id}", ([AsParameters] CreateFruitModel model) =>
{
    return model.Fruit;
}).WithName("CreateFruit")
.WithSummary("Create a fruit");

app.Run();

record struct CreateFruitModel
    ([FromRoute, SwaggerParameter(Description = "The id of the fruit that will be created", Required = true)] string Id,
    [FromBody, SwaggerRequestBody("Description for Body")] Fruit Fruit);
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
record Fruit(string Name);

namespace WebApi
{
    public partial class Program
    {
        // Expose the Program class for use with WebApplicationFactory<T>
    }
}
