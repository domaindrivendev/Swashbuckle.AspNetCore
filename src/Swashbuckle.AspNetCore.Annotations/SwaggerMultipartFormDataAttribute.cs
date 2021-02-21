using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Adds or enriches a multipart/form-data boundary.
    /// <remark>
    /// Use when model binding is not an option, i.e the data is read directly from the underlying stream. Defaults to an optional binary file boundary, named "file".
    /// </remark>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerMultiPartFormDataAttribute : Attribute
    {
        public SwaggerMultiPartFormDataAttribute() : this("file", false)
        {
        }

        public SwaggerMultiPartFormDataAttribute(string name) : this(name, false)
        {
        }

        public SwaggerMultiPartFormDataAttribute(bool isRequired) : this("file", isRequired)
        {
        }

        public SwaggerMultiPartFormDataAttribute(string name, bool required)
        {
            Name = name;
            Required = required;
        }

        /// <summary>
        /// Name of the boundary.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines whether the boundary is mandatory.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Type of the boundary as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Format of the boundary's type as defined in OpenApi specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        public string Format { get; set; }
    }
}
