namespace Swashbuckle.AspNetCore.SwaggerUI;

internal static class ResourceHelper
{
    public static Stream GetEmbeddedResource(string fileName)
    {
        return typeof(ResourceHelper).Assembly
            .GetManifestResourceStream($"Swashbuckle.AspNetCore.SwaggerUI.{fileName}");
    }
}
