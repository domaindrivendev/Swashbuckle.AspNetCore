namespace Swashbuckle.AspNetCore.Annotations.Test
{
    [SwaggerSchema("Description for SwaggerAnnotatedType", Required = new[] { "StringWithSwaggerSchemaAttribute" }, Title = "Title for SwaggerAnnotatedType")]
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public class SwaggerAnnotatedType
    {
        [SwaggerSchema("Description for StringWithSwaggerSchemaAttribute", Format = "date", ReadOnly = true, WriteOnly = true, Nullable = false)]
        public string StringWithSwaggerSchemaAttribute { get; set; }

        [SwaggerSchema("Description for StringWithSwaggerSchemaAttributeDescriptionOnly")]
        public string StringWithSwaggerSchemaAttributeDescriptionOnly { get; set; }

        [SwaggerParameter("Description for StringWithSwaggerParameterAttribute", Required = true)]
        public string StringWithSwaggerParameterAttribute { get; set; }

        [SwaggerRequestBody("Description for StringWithSwaggerRequestBodyAttribute", Required = true)]
        public string StringWithSwaggerRequestBodyAttribute { get; set; }
    }

    [SwaggerSchema("Description for SwaggerAnnotatedStruct", Required = new[] { "StringWithSwaggerSchemaAttribute" }, Title = "Title for SwaggerAnnotatedStruct")]
    [SwaggerSchemaFilter(typeof(VendorExtensionsSchemaFilter))]
    public struct SwaggerAnnotatedStruct
    {
        [SwaggerSchema("Description for StringWithSwaggerSchemaAttribute", Format = "date", ReadOnly = true, WriteOnly = true, Nullable = false)]
        public string StringWithSwaggerSchemaAttribute { get; set; }
    }
}