using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Enriches Operation metadata for a given action method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerOperationAttribute : Attribute
    {
        public SwaggerOperationAttribute(string summary = null, string description = null)
        {
            Summary = summary;
            Description = description;
        }

        /// <summary>
        /// A short summary of what the operation does. For maximum readability in the swagger-ui,
        /// this field SHOULD be less than 120 characters.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// A verbose explanation of the operation behavior. GFM syntax can be used for rich text representation.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unique string used to identify the operation. The id MUST be unique among all operations described
        /// in the API. Tools and libraries MAY use the operationId to uniquely identify an operation, therefore,
        /// it is recommended to follow common programming naming conventions.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// A list of tags for API documentation control. Tags can be used for logical grouping of operations
        /// by resources or any other qualifier.
        /// </summary>
        public string[] Tags { get; set; }
    }
}