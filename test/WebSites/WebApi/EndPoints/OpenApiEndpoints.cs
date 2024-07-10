using Microsoft.AspNetCore.Mvc;

namespace WebApi.EndPoints
{
    /// <summary>
    /// Class of Extensions to add WithOpenApiEndpoints
    /// </summary>
    public static class OpenApiEndpoints
    {
        /// <summary>
        /// Extension to add WithOpenApiEndpoints
        /// </summary>
        public static IEndpointRouteBuilder MapWithOpenApiEndpoints(this IEndpointRouteBuilder app)
        {
            string[] summaries = [
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            ];

            var group = app.MapGroup("/WithOpenApi").WithTags("WithOpenApi");

            group.MapGet("weatherforecast", () =>
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

            group.MapPost("/multipleForms", ([FromForm] Person person, [FromForm] Address address) =>
            {
                TypedResults.NoContent();
            })
            .WithOpenApi()
            .DisableAntiforgery();

            group.MapPost("/IFromFile", (IFormFile file) =>
            {
                return file.FileName;
            }).WithOpenApi()
            .DisableAntiforgery();

            group.MapPost("/IFromFileCollection", (IFormFileCollection collection) =>
            {
                return $"{collection.Count} {string.Join(',', collection.Select(f => f.FileName))}";
            }).WithOpenApi()
            .DisableAntiforgery();

            return app;
        }
    }
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
    record class Person(string FirstName, string LastName);

    record class Address(string Street, string City, string State, string ZipCode);
}
