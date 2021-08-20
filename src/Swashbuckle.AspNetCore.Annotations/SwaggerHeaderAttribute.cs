using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Adds or enriches Parameter metadata for a given action method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerHeaderAttribute : Attribute
    {
        public SwaggerHeaderAttribute(string name, bool required = false, string description = null, string type = null, string format = null)
        {
            Name = name;
            Required = required;
            Description = description;
            Type = type;
            Format = format;
        }

        /// <summary>
        /// Name of the header.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines whether the parameter is mandatory.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// A brief description of the parameter. This could contain examples of use.
        /// GFM syntax can be used for rich text representation
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Data type of the header as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Format of header's data type as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        public string Format { get; set; }
    }
}
