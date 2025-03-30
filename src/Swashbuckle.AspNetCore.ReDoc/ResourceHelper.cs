namespace Swashbuckle.AspNetCore.ReDoc;

internal static class ResourceHelper
{
    public static Stream GetEmbeddedResource(string fileName)
    {
        return typeof(ResourceHelper).Assembly
            .GetManifestResourceStream($"Swashbuckle.AspNetCore.ReDoc.{fileName}");
    }
}
