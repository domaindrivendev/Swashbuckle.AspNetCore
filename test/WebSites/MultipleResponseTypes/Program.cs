using Microsoft.AspNetCore.Mvc;
#if !NET11_0_OR_GREATER
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection.Extensions;
#endif
using MultipleResponseTypes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new() { Title = "MultipleResponseTypes", Version = "v1" }));

// Exercises Swashbuckle's multiple-response-types-per-status-code handling end-to-end.
// From .NET 11, ASP.NET Core's API explorer reports every response type for a status code.
// Earlier versions only report the last one, so a second response type is injected after
// the default providers to produce the same document for GET /animals/{id}.
#if !NET11_0_OR_GREATER
builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IApiDescriptionProvider, MultipleResponseTypesApiDescriptionProvider>());
#endif

var app = builder.Build();

app.UseSwagger();

// An endpoint that advertises two response types (Cat and Dog) for the same 200 status code,
// so the generated document contains an "anyOf" of the two schemas.
#if NET11_0_OR_GREATER
app.MapGet("/animals/{id}", (int id) => new Cat("Whiskers", 9))
   .Produces<Cat>(StatusCodes.Status200OK)
   .Produces<Dog>(StatusCodes.Status200OK);
#else
app.MapGet("/animals/{id}", (int id) => new Cat("Whiskers", 9))
   .Produces<Cat>(StatusCodes.Status200OK);
#endif

// An endpoint that advertises a different response type (Cat) to the type returned by its
// handler (Dog). From .NET 11, ASP.NET Core's API explorer reports both types, so the generated
// document contains an "anyOf" of the two schemas. Earlier versions only report Cat.
app.MapGet("/pets/{id}", (int id) => new Dog("Rex", true))
   .Produces<Cat>(StatusCodes.Status200OK);

app.MapControllers();

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
    /// A controller with an action that advertises two response types for the same status code.
    /// From .NET 11, ASP.NET Core's API explorer reports both types, so the generated document
    /// contains an "anyOf" of the two schemas. Earlier versions only report the last one (Dog).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ShelterController : ControllerBase
    {
        /// <summary>
        /// Gets a random animal from the shelter.
        /// </summary>
        [HttpGet("animal")]
        [ProducesResponseType<Cat>(StatusCodes.Status200OK)]
        [ProducesResponseType<Dog>(StatusCodes.Status200OK)]
        public IActionResult GetRandom() => Ok(new Cat("Whiskers", 9));
    }

#if !NET11_0_OR_GREATER
    /// <summary>
    /// Injects a second <see cref="ApiResponseType"/> for status code 200 after the default
    /// API explorer has run, simulating multiple response types for a single status code.
    /// </summary>
    internal sealed class MultipleResponseTypesApiDescriptionProvider : IApiDescriptionProvider
    {
        // Runs after the framework's providers (which use negative Order values) so the
        // descriptions are already populated and can be augmented.
        public int Order => 0;

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var apiDescription in context.Results)
            {
                if (apiDescription.RelativePath != "animals/{id}")
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
#endif

    /// <summary>
    /// Expose the Program class for use with <c>WebApplicationFactory</c>.
    /// </summary>
    public partial class Program;
}
