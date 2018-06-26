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
        /// <param name="description">The parameter description</param>
        public SwaggerParameterAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets or sets the parameter description
        /// </summary>
        public string Description { get;  set; }
    }
}