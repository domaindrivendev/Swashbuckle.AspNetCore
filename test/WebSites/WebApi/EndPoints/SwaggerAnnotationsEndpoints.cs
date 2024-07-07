using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
namespace WebApi.EndPoints
{
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
            var group = app.MapGroup("/annotations").WithTags("Annotations");

            group.MapPost("/fruit/{id}", CreateFruit);

            return app;
        }

        [SwaggerResponse(200, "Description for response", typeof(Fruit))]
        [SwaggerOperation("CreateFruit", "Create a fruit")]
        private static Fruit CreateFruit([AsParameters] CreateFruitModel createFruitModel) => createFruitModel.Fruit;
    }
    record struct CreateFruitModel
    ([FromRoute, SwaggerParameter(Description = "The id of the fruit that will be created", Required = true)] string Id,
    [FromBody, SwaggerRequestBody("Description for Body")] Fruit Fruit);

    [SwaggerSchema("Description for Schema")]
    record Fruit(string Name);
}
