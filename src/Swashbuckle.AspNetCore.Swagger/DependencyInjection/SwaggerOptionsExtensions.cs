using System;
using Swashbuckle.AspNetCore.Swagger;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for helping with configuring the SwaggerOptions
/// </summary>
public static class SwaggerOptionsExtensions
{
    /// <summary>
    /// Sets a custom Swagger document serializer to use.
    /// </summary>
    /// <remarks>For the CLI tool to be able to use this, this needs to be configured for use in the service collection of your application.</remarks>
    /// <typeparam name="TDocumentSerializer">The custom Swagger document serializer class</typeparam>
    /// <param name="swaggerOptions">The options to configure for serializer for.</param>
    /// <param name="constructorParameters">Theparameters to pass into the constructor of the custom Swagger document serializer implementation.</param>
    public static void SetCustomDocumentSerializer<TDocumentSerializer>(
        this SwaggerOptions swaggerOptions,
        params object[] constructorParameters)
        where TDocumentSerializer : ISwaggerDocumentSerializer
    {
        if (swaggerOptions == null)
        {
            return;
        }
        swaggerOptions.CustomDocumentSerializer = (TDocumentSerializer)Activator.CreateInstance(typeof(TDocumentSerializer), constructorParameters);
    }
}
