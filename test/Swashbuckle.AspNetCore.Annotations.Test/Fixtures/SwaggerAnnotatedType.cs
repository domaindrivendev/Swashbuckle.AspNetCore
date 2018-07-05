namespace Swashbuckle.AspNetCore.Annotations.Test
{
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedClass
    {
        public string Property { get; set; }
    }

    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public struct SwaggerAnnotatedStruct
    {
        public string Property { get; set; }
    }
}