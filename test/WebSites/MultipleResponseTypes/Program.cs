using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MultipleResponseTypes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "MultipleResponseTypes", Version = "v1" }));

// Exercises Swashbuckle's multiple-response-types-per-status-code handling end-to-end.
// ASP.NET Core's built-in API explorer collapses duplicate status codes on the currently
// supported runtimes, so a second response type is injected below the default provider to
// mimic what a C# union return type (.NET 11+) will surface.
builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, UnionResponseApiDescriptionProvider>());

var app = builder.Build();

app.UseSwagger();

// A single endpoint that advertises one response type (Cat) via metadata; the injected
// provider adds a second (Dog) for the same 200 status code so the generated document
// contains an "anyOf" of the two schemas.
app.MapGet("/animals/{id}", (int id) => new Cat("Whiskers", 9))
   .Produces<Cat>(StatusCodes.Status200OK);

app.Run();

namespace MultipleResponseTypes
{
    /// <summary>
    /// A response type for the <c>GET /animals/{id}</c> endpoint.
    /// </summary>
    public record Cat(string Name, int LivesLeft);

    /// <summary>
    /// A second response type surfaced for the same status code as <see cref="Cat"/>.
    /// </summary>
    public record Dog(string Name, bool GoodBoy);

    /// <summary>
    /// Injects a second <see cref="ApiResponseType"/> for status code 200 after the default
    /// API explorer has run, simulating multiple response types for a single status code.
    /// </summary>
    internal sealed class UnionResponseApiDescriptionProvider : IApiDescriptionProvider
    {
        // Runs after the framework's providers (which use negative Order values) so the
        // descriptions are already populated and can be augmented.
        public int Order => 0;

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                if (apiDescription.RelativePath?.StartsWith("animals/", StringComparison.OrdinalIgnoreCase) != true)
                {
                    continue;
                }

                var existing = apiDescription.SupportedResponseTypes
                    .FirstOrDefault((responseType) => responseType.StatusCode == StatusCodes.Status200OK);

                if (existing is null)
                {
                    continue;
                }

                apiDescription.SupportedResponseTypes.Add(new ApiResponseType
                {
                    StatusCode = StatusCodes.Status200OK,
                    Type = typeof(Dog),
                    ApiResponseFormats = [.. existing.ApiResponseFormats.Select((format) => new ApiResponseFormat { MediaType = format.MediaType, Formatter = format.Formatter })],
                });
            }
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }
    }

    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>.
    /// </summary>
    public partial class Program;
}
