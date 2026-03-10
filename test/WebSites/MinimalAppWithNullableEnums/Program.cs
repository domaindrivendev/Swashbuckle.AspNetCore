using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((p) =>
{
    p.SupportNonNullableReferenceTypes();
    p.UseInlineDefinitionsForEnums();
    p.UseAllOfToExtendReferenceSchemas();
    p.UseAllOfForInheritance();
});

var app = builder.Build();

app.UseSwagger((p) => p.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1);
app.UseSwaggerUI();

app.MapGet("/weatherforecast", ([FromBody] Parent a) => { })
   .WithName("GetWeatherForecast");

app.Run();

public class Parent
{
    public Child? Child { get; set; }

    public Category? Category { get; set; }
}

public class Child
{
    public string Name { get; set; } = string.Empty;
}

public enum Category
{
    One,
    Two,
    Three,
}

namespace MinimalAppWithNullableEnums
{
    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>
    /// </summary>
    public partial class Program
    {
    }
}
