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

    public class SwaggerPropertyAnnotedClass
    {
        [SwaggerProperty(typeof(SerializedPropertyAnnotedClass))]
        public DeclaredPropertyAnnotedClass Property { get; set; }
    }

    public class DeclaredPropertyAnnotedClass
    {
        public string NotSerializedProperty { get; set; }
    }

    public class SerializedPropertyAnnotedClass
    {
        public string SerializedProperty { get; set; }
    }
}