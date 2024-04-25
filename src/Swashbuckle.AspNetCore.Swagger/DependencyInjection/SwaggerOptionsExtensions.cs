using System;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

public static class SwaggerOptionsExtensions
{
    /// <summary>
    /// Sets a custom swagger document serializer.
    /// </summary>
    /// <remarks>To work for the CLI tool, this needs to be performed during ConfigureServices.</remarks>
    /// <param name="swaggerOptions"></param>
    /// <param name="constructorParameters">parameters to pass into the constructor of the custom swagger document serializer.</param>
    public static void SetCustomDocumentSerializer<TDocumentSerializer>(
        this SwaggerOptions swaggerOptions,
        params object[] constructorParameters)
        where TDocumentSerializer : ISwaggerDocumentSerializer
    {
        swaggerOptions.CustomDocumentSerializer = (TDocumentSerializer)Activator.CreateInstance(typeof(TDocumentSerializer), constructorParameters);
    }
}
