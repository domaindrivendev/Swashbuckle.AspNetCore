namespace Swashbuckle.AspNetCore.SwaggerGen.Test
{
    [SwaggerOperationFilter(typeof(VendorExtensionsOperationFilter))]
    public class SwaggerAnnotatedController
    {
        public void ReturnsVoid()
        { }
    }
}