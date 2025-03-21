using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.EndPoints;

/// <summary>
/// Class of Extensions to add AnnotationsEndpoints
/// </summary>
public static class SwaggerAnnotationsEndpoints
{
    /// <summary>
    /// Extension to add AnnotationsEndpoints
    /// </summary>
    public static IEndpointRouteBuilder MapAnnotationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/annotations")
            .WithTags("Annotations")
            .DisableAntiforgery();

        group.MapPost("/fruit/{id}", CreateFruit)
             .WithOpenApi();

        group.MapPost("/singleForm", ([FromForm] PersonAnnotated person) =>
        {
            return person.ToString();
        });

        group.MapPost("/multipleForms", ([FromForm] PersonAnnotated person, [FromForm] AddressAnnotated address) =>
        {
            return $"{person} {address}";
        });

        group.MapPost("/IFromFileAndString", ([SwaggerSchema("Description for File")] IFormFile file, [FromForm] string tags) =>
        {
            return $"{file.FileName}{tags}";
        });

        group.MapPost("/IFromFileAndEnum", (IFormFile file, [FromForm, SwaggerSchema("Description for DateTimeKind")] DateTimeKind dateTimeKind) =>
        {
            return $"{file.FileName}{dateTimeKind}";
        });

        group.MapPost("/IFromObjectAndString", ([FromForm] PersonAnnotated person, [FromForm] string tags) =>
        {
            return $"{person}{tags}";
        });

        group.MapGet("/AsParameters", ([AsParameters] AsParametersRecord record) =>
        {
            return record;
        });

        return app;
    }

    [SwaggerResponse(200, "Description for response", typeof(Fruit))]
    [SwaggerOperation("CreateFruit", "Create a fruit")]
    private static Fruit CreateFruit([AsParameters] CreateFruitModel createFruitModel) => createFruitModel.Fruit;
}
record PersonAnnotated([property: SwaggerSchema("Description for FirstName")] string FirstName, [property: SwaggerSchema("Description for LastName")] string LastName);
record AddressAnnotated([property: SwaggerSchema("Description for Street")] string Street, string City, string State, string ZipCode);
record struct CreateFruitModel
([FromRoute, SwaggerParameter(Description = "The id of the fruit that will be created", Required = true)] string Id,
[FromBody, SwaggerRequestBody("Description for Body")] Fruit Fruit);

[SwaggerSchema("Description for Schema")]
record Fruit(string Name);

record class AsParametersRecord(
    [SwaggerParameter(Description = "Description")] Guid? paramOne,
    Guid paramTwo,
    DateTime? paramThree,
    DateTime paramFour,
    DateOnly? paramFive,
    DateOnly paramSix,
    TimeOnly? paramSeven,
    TimeOnly paramEight,
    DateTimeKind? paramNine,
    DateTimeKind paramTen,
    decimal? paramEleven,
    decimal paramTwelve);
