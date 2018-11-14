using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Adds or enriches Response metadata for a given action method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerResponseAttribute : Attribute
    {
        public SwaggerResponseAttribute(int statusCode, string description = null, Type type = null) {
            StatusCode = statusCode;
            Description = description;
            Type = type;
        }

        /// <summary>
        /// Status code of the response.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// A short description of the response. GFM syntax can be used for rich text representation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Defines the type returned by the response.
        /// </summary>
        public Type Type { get; }
    }
}
