using System.ComponentModel;
using System.Text.Json.Serialization;
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

            var group = app.MapGroup("/WithOpenApi")
                .WithTags("WithOpenApi")
                .DisableAntiforgery();

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
                return $"{person} {address}";
            })
            .WithOpenApi();

            group.MapPost("/IFromFile", (IFormFile file, string queryParameter) =>
            {
                return $"{file.FileName}{queryParameter}";
            }).WithOpenApi(o =>
            {
                var parameter = o.Parameters?.FirstOrDefault(p => p.Name.Equals("queryParameter", StringComparison.OrdinalIgnoreCase));
                if (parameter is not null)
                {
                    parameter.Description = $"{parameter.Name} Description";
                }
                return o;
            });

            group.MapPost("/IFromFileCollection", (IFormFileCollection collection) =>
            {
                return $"{collection.Count} {string.Join(',', collection.Select(f => f.FileName))}";
            }).WithOpenApi();

            group.MapPost("/IFromBody", (OrganizationCustomExchangeRatesDto dto) =>
            {
                return $"{dto}";
            }).WithOpenApi();

            group.MapPost("/IFromFileAndString", (IFormFile file, [FromForm] string tags) =>
            {
                return $"{file.FileName}{tags}";
            }).WithOpenApi();

            group.MapPost("/IFromFileAndEnum", (IFormFile file, [FromForm] DateTimeKind dateTimeKind) =>
            {
                return $"{file.FileName}{dateTimeKind}";
            }).WithOpenApi();

            group.MapPost("/IFromObjectAndString", ([FromForm] Person person, [FromForm] string tags) =>
            {
                return $"{person}{tags}";
            }).WithOpenApi();

            app.MapGet("/TypeWithTryParse/{tryParse}", (TypeWithTryParse tryParse) =>
            {
                return tryParse.Name;
            }).WithOpenApi();

            return app;
        }
    }
    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
    record class Person(string FirstName, string LastName);

    record class Address(string Street, string City, string State, string ZipCode);
    sealed record OrganizationCustomExchangeRatesDto([property: JsonRequired] CurrenciesRate[] CurrenciesRates, [property: ReadOnly(true)] bool IsUpdated);
    sealed record CurrenciesRate([property: JsonRequired, Description("Currency From")] string CurrencyFrom, [property: JsonRequired] string CurrencyTo, double Rate);
    record TypeWithTryParse(string Name)
    {
        public static bool TryParse(string value, out TypeWithTryParse? result)
        {
            if (value is null)
            {
                result = null;
                return false;
            }

            result = new TypeWithTryParse(value);
            return true;
        }
    }
}
