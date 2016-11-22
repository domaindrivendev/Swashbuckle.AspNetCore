using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.SwaggerGen.Annotations
{
    /// <summary>
    /// Response attribute that extends the standard <see cref="ProducesResponseTypeAttribute"/> allowing
    /// additional Swagger annotation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute derives from <see cref="ProducesResponseTypeAttribute"/> so you don't
    /// have to put duplicate/redundant attributes on a controller or action to serve both Swagger
    /// and standard ASP.NET API description purposes - just use the extended
    /// Swagger response attribute and it works for both.
    /// </para>
    /// </remarks>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerProducesResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="SwaggerProducesResponseTypeAttribute"/>.
        /// </summary>
        /// <param name="statusCode">
        /// The HTTP response status code.
        /// </param>
        /// <param name="type">
        /// The <see cref="Type"/> of object that is going to be written in the response. If no type is returned,
        /// use <see langword="null" /> or <c>typeof(void)</c>.
        /// </param>
        /// <param name="description">
        /// A <see cref="string"/> containing description of the response that should appear in the documentation.
        /// </param>
        public SwaggerProducesResponseTypeAttribute(int statusCode, Type type = null, string description = null)
            : base(type == null ? typeof(void) : type, statusCode)
        {
            this.Description = description;
        }

        /// <summary>
        /// Gets or sets the response description.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> containing description of the response that should appear in the documentation.
        /// </value>
        public string Description { get; set; }
    }
}
