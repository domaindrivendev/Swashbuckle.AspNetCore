using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Adds Tag metadata for a given controller (i.e. the controller name tag)
    /// </summary>
    /// <remarks>
    /// Don't use this attribute if you're tagging Operations with something other than controller name
    /// e.g. if you're customizing the tagging behavior with TagActionsBy.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerTagAttribute : Attribute
    {
        public SwaggerTagAttribute(string description = null, string externalDocsUrl = null)
        {
            Description = description;
            ExternalDocsUrl = externalDocsUrl;
        }

        /// <summary>
        /// A short description for the tag. GFM syntax can be used for rich text representation.
        /// </summary>
        public string Description { get;  }

        /// <summary>
        /// A URL for additional external documentation. Value MUST be in the format of a URL.
        /// </summary>
        public string ExternalDocsUrl { get; }
    }
}