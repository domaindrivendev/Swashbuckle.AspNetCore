using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Enriches Parameter metadata for "path", "query" or "header" bound parameters or properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerParameterAttribute : Attribute
    {
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
        /// Determines whether the parameter is mandatory. If the parameter is in "path",
        /// it will be required by default as Swagger does not allow optional path parameters
        /// </summary>
        public bool Required
        {
            get { throw new InvalidOperationException($"Use {nameof(RequiredFlag)} instead"); }
            set { RequiredFlag = value; }
        }

        internal bool? RequiredFlag { get; set; }

        /// <summary>
        /// Describes how the parameter value will be serialized depending on the type of
        /// the parameter value. Default values (based on value of in): for query - form;
        /// for path - simple; for header - simple; for cookie - form.
        /// </summary>
        public string Style
        {
            get { throw new InvalidOperationException($"Use {nameof(ParameterStyle)} instead"); }
            set
            {
                ParameterStyle = Enum.TryParse(value, ignoreCase: true, out ParameterStyle result) ? result :
                    throw new InvalidOperationException(
                        message: $"Style '{value}' not defined in OpenAPI specification");
            }
        }

        internal ParameterStyle? ParameterStyle { get; set; }

        /// <summary>
        /// When this is true, parameter values of type array or object generate separate parameters for
        /// each value of the array or key-value pair of the map. For other types of parameters this property
        /// has no effect. When style is form, the default value is true. For all other styles,
        /// the default value is false.
        /// </summary>
        public bool Explode
        {
            get { throw new InvalidOperationException($"Use {nameof(ExplodeFlag)} instead"); }
            set { ExplodeFlag = value; }
        }

        internal bool? ExplodeFlag { get; set; }
    }
}
