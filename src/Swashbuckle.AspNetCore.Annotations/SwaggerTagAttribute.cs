using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Defines document tags and related descriptions
    /// </summary>
    /// <remarks>
    /// Tags defined on a controller will be added to the main swagger object.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SwaggerTagAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerTagAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the tag, if an empty or whitespace name is given it wont be included</param>
        /// <param name="description">An optional description for the tag</param>
        /// <param name="externalDocUrl">An optional external document url</param>
        public SwaggerTagAttribute(string name, string description = null, string externalDocUrl = null)
        {
            this.Name = name;
            this.Description = description;
            this.Tag = new Tag { Name = this.Name, Description = this.Description };

            if (externalDocUrl != null)
            {
                this.Tag.ExternalDocs = new ExternalDocs { Url = externalDocUrl };
            }
        }

        /// <summary>
        /// Gets the name of the Tag
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Description description for the tag
        /// </summary>
        public string Description { get;  }

        /// <summary>
        /// Gets the Tag that is going to be added to the Document model.
        /// </summary>
        public Tag Tag { get; }
    }
}