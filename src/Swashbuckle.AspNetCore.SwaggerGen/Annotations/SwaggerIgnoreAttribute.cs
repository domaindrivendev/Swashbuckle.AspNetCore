using System;

// ReSharper disable once CheckNamespace
namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Causes the annotated member to be ignored during schema generation.
    /// Does not alter serialization behavior.
    /// </summary>
    /// <remarks>
    /// Can be used in combination with <see cref="System.Text.Json.Serialization.JsonExtensionDataAttribute"/>
    /// to capture and invalidate unsupported properties.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
    public sealed class SwaggerIgnoreAttribute : Attribute { }
}
