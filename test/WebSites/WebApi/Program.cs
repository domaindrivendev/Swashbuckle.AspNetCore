using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using WebApi.EndPoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JsonOptions>(
    opt => opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.IncludeXmlComments(Assembly.GetExecutingAssembly());
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

app.MapAnnotationsEndpoints()
   .MapWithOpenApiEndpoints()
   .MapXmlCommentsEndpoints();

app.Run();

namespace WebApi
{
    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>
    /// </summary>
    public partial class Program
    {
    }
}
