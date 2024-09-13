using System.IO;
using System.Reflection;

namespace Swashbuckle.AspNetCore.Scalar;

internal static class ResourceHelper
{
    public static Stream GetEmbeddedResource(string fileName)
    {
        return typeof(ResourceHelper).GetTypeInfo().Assembly
            .GetManifestResourceStream($"Swashbuckle.AspNetCore.Scalar.{fileName}");
    }
}
