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
        /// Describes how the parameter value will be serialized based on its type. 
        /// If not set, default values are for: query - form; path - simple; header - simple; cookie - form.
        /// </summary>
        public string Style
        {
            get { throw new InvalidOperationException($"Use {nameof(ParameterStyle)} instead"); }
            set
            {
                ParameterStyle = Enum.TryParse(value, ignoreCase: true, out ParameterStyle result) ? result :
                    throw new NotSupportedException($"'{value}' style is not supported");
            }
        }

        internal ParameterStyle? ParameterStyle { get; set; }

        /// <summary>
        /// When true, array and object parameters are split into separate parameters. This has no 
        /// effect on other parameter types. 
        /// The default is true for form style and false for all other styles.
        /// </summary>
        public bool Explode
        {
            get { throw new InvalidOperationException($"Use {nameof(ExplodeFlag)} instead"); }
            set { ExplodeFlag = value; }
        }

        internal bool? ExplodeFlag { get; set; }
    }
}
