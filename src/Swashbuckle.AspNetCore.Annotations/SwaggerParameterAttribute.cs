using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Enriches Parameter metadata for a given top-level parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class SwaggerParameterAttribute : Attribute
    {
        private bool _required;

        public SwaggerParameterAttribute(string description = null)
        {
            Description = description;
        }

        /// <summary>
        /// A brief description of the parameter. This could contain examples of use.
        /// GFM syntax can be used for rich text representation
        /// </summary>
        public string Description { get;  set; }

        /// <summary>
        /// Determines whether this parameter is mandatory. If the parameter is in "path",
        /// it will be required by default as Swagger does not allow optional path parameters
        /// </summary>
        public bool Required
        {
            get { return _required; }
            set
            {
                RequiredProvided = true;
                _required = value;
            }
        }

        internal bool RequiredProvided { get; set; }
    }
}