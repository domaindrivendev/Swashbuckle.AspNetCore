using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations;

/// <summary>
/// Adds or enriches Response metadata for a given action method
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class SwaggerResponseAttribute : ProducesResponseTypeAttribute
{
    public SwaggerResponseAttribute(int statusCode, string description = null, Type type = null)
        : base(type ?? typeof(void), statusCode)
    {
        Description = description;
    }

    public SwaggerResponseAttribute(int statusCode, string description = null, Type type = null, params string[] contentTypes)
        : base(type ?? typeof(void), statusCode)
    {
        Description = description;
        ContentTypes = contentTypes;
    }

#if !NET10_0_OR_GREATER
    /// <summary>
    /// A short description of the response. GFM syntax can be used for rich text representation.
    /// </summary>
    public string Description { get; set; }
#endif

    /// <summary>
    /// A collection of MIME types that the response can be produced with.
    /// </summary>
    public string[] ContentTypes { get; set; }
}
