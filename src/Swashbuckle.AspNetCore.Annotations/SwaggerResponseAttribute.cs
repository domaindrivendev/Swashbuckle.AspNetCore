using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations
{
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

        /// <summary>
        /// A short description of the response. GFM syntax can be used for rich text representation.
        /// </summary>
        public string Description { get; set; }
    }
}
