using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

// This exists just to test and verify that the MapSwagger and MapSwaggerUI works as expected.

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "WebApi", Version = "v1" }));

// Authentication and Authorization are added to verify that MapSwaggerUI and MapReDoc work as expected when RequireAuthorization is used.
builder.Services.AddAuthentication().AddBearerToken();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// begin-snippet: SwaggerUI-MapSwaggerUI
app.MapSwagger();
app.MapSwaggerUI("swagger", options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs"));
// end-snippet

// begin-snippet: SwaggerUI-MapSwaggerUI-RequireAuthorization
app.MapSwaggerUI("swagger-auth")
   .RequireAuthorization(); // Remember to also add RequireAuthorization to MapSwagger.
// end-snippet


// Also Map redoc in the same file
// begin-snippet: Redoc-MapReDoc
app.MapReDoc();
// end-snippet

// begin-snippet: Redoc-MapReDoc-RequireAuthorization
app.MapReDoc("redoc-auth")
   .RequireAuthorization(); // Remember to also add RequireAuthorization to MapSwagger
// end-snippet

app.UseHttpsRedirection();

string[] summaries =
[
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
            new
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }
        )
        .ToArray();
    return forecast;
});

app.Run();

namespace WebApi.Map
{
    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>
    /// </summary>
    public partial class Program
    {
    }
}
