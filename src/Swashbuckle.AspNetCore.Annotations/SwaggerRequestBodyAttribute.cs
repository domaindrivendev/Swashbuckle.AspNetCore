using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Enriches RequestBody metadata for "body" bound parameters or properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerRequestBodyAttribute : Attribute
    {
        public SwaggerRequestBodyAttribute(string description = null)
        {
            Description = description;
        }

        /// <summary>
        /// A brief description of the requestBody. This could contain examples of use.
        /// GFM syntax can be used for rich text representation
        /// </summary>
        public string Description { get;  set; }

        /// <summary>
        /// Determines whether the requestBody is mandatory. If the parameter is in "path",
        /// it will be required by default as Swagger does not allow optional path parameters
        /// </summary>
        public bool Required
        {
            get { throw new InvalidOperationException($"Use {nameof(RequiredFlag)} instead"); }
            set { RequiredFlag = value; }
        }

        internal bool? RequiredFlag { get; set; }
    }
}