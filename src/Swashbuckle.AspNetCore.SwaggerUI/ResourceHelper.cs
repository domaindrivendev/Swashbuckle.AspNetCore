using System.IO;
using System.Reflection;

namespace Swashbuckle.AspNetCore.SwaggerUI;

internal class ResourceHelper
{
    public static Stream GetEmbeddedResource(string fileName)
    {
        return typeof(ResourceHelper).GetTypeInfo().Assembly
            .GetManifestResourceStream($"Swashbuckle.AspNetCore.SwaggerUI.{fileName}");
    }
}
