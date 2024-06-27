namespace WebApi.EndPoints
{

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

            app.MapGet("/AsParameters", ([AsParameters] AsParametersArgument request) => "Hello World!");

            return app;
        }
        /// <summary>
        /// Returns a specific product
        /// </summary>
        ///  <param name="id" example="111">The product id</param>
        /// <response code="200">A Product Id</response>
        private static Product GetProduct(int id) => new Product { Id = id, Description = "A product" };
    }
    /// <summary>
    /// Represents a product
    /// </summary>
    public class Product
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
    internal struct AsParametersArgument
    {
        /// <summary>
        /// This is a property with the number one - This is nowhere in SwaggerUI
        /// </summary>
        public string PropertyOne { get; set; }

        /// <summary>
        /// This is a property with the number two - This is nowhere in SwaggerUI
        /// </summary>
        public string PropertyTwo { get; set; }
    }
}
