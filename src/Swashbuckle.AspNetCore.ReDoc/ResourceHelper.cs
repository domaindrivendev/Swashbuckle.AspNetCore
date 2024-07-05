﻿using System.IO;
using System.Reflection;

namespace Swashbuckle.AspNetCore.ReDoc;

internal static class ResourceHelper
{
    public static Stream GetEmbeddedResource(string fileName)
    {
        return typeof(ResourceHelper).GetTypeInfo().Assembly
            .GetManifestResourceStream($"Swashbuckle.AspNetCore.ReDoc.{fileName}");
    }
}
