namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedClass
    {
        public string Property { get; set; }
    }

    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedStruct
    {
        public string Property { get; set; }
    }
}