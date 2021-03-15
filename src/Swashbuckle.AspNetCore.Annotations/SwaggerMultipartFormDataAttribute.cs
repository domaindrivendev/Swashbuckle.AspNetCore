using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Adds or enriches a multipart/form-data boundary.
    /// <remark>
    /// Use when model binding is not an option, i.e the data is read directly from the underlying stream.
    /// Defaults to an optional binary file boundary, named "file".
    /// </remark>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerMultiPartFormDataAttribute : Attribute
    {

        public SwaggerMultiPartFormDataAttribute(string name = null, bool required = false, string type = null, string collectionType = null, string format = null)
        {
            Name = name;
            Required = required;
            Type = type;
            CollectionType= collectionType;
            Format = format;
        }
        private string _collectionType;

        /// <summary>
        /// Name of the boundary.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Determines whether the boundary is mandatory.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Data type of the boundary as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        /// <remarks>
        /// Supports "array" type for defining a collection of multiple boundaries.
        /// </remarks>
        public string Type { get; set; }

        /// <summary>
        /// Data type of the boundary in a collection as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        /// <remarks>
        /// Used when <seealso cref="Type"/> is "array".
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when <seealso cref="CollectionType"/> is set to "array".</exception>
        public string CollectionType
        {
            get => _collectionType;

            set
            {
                if (string.CompareOrdinal(value, "array") == 0)
                {
                    throw new InvalidOperationException($"{nameof(CollectionType)} cannot be an array.");
                }

                _collectionType = value;
            }
        }

        /// <summary>
        /// Format of boundary's data type as defined in OpenAPI specifications: https://swagger.io/docs/specification/data-models/data-types/
        /// </summary>
        public string Format { get; set; }
    }
}
