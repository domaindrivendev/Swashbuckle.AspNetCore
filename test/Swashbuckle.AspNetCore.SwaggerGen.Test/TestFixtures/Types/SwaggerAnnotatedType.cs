namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedType
    {
        public string Property { get; set; }
    }
}