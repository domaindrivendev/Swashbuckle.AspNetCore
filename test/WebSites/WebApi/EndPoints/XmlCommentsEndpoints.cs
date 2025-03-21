namespace WebApi.EndPoints;


/// <summary>
/// Class of Extensions to add XmlEndpoints
/// </summary>
public static class XmlCommentsEndpoints
{
    /// <summary>
    /// Extension to add AnnotationsEndpoints
    /// </summary>
    public static IEndpointRouteBuilder MapXmlCommentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/XmlComments").WithTags("Xml");

        group.MapGet("/Car/{id}", GetProduct);

        return app;
    }
    /// <summary>
    /// Returns a specific product
    /// </summary>
    ///  <param name="id" example="111">The product id</param>
    /// <response code="200">A Product Id</response>
    private static Product GetProduct(int id) => new() { Id = id, Description = "A product" };
}
/// <summary>
/// Represents a product
/// </summary>
internal class Product
{
    /// <summary>
    /// Uniquely identifies the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Describes the product
    /// </summary>
    public string? Description { get; set; }
}
