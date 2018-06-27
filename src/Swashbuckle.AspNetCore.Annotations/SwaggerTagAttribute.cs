using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Provides additional metadata for the default operation tag (i.e. controller name)
    /// </summary>
    /// <remarks>
    /// Don't use this if you're customizing the default tag for operations via TagActionsBy.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SwaggerTagAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerTagAttribute"/> class.
        /// </summary>
        /// <param name="description">A tag description</param>
        /// <param name="externalDocsUrl">A url to external docs for the tag</param>
        public SwaggerTagAttribute(string description = null, string externalDocsUrl = null)
        {
            Description = description;
            ExternalDocsUrl = externalDocsUrl;
        }

        /// <summary>
        /// Gets the tag description
        /// </summary>
        public string Description { get;  }

        /// <summary>
        /// Gets the external docs url
        /// </summary>
        public string ExternalDocsUrl { get; }
    }
}