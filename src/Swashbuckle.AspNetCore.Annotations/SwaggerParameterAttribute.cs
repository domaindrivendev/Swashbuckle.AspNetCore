using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Defines parameter metadata such as the description.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class SwaggerParameterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerParameterAttribute"/> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        public SwaggerParameterAttribute(string description = null, bool required = false)
        {
            this.Description = description;
            this.Required = required;
        }

        /// <summary>
        /// Gets or sets the description of the parameter
        /// </summary>
        public string Description { get;  set; }

        /// <summary>
        /// Gets or sets a value indicating whether the annotated parameter is required.
        /// </summary>
        public bool Required { get; set; }
    }
}