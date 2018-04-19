using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SwaggerTagAttribute : Attribute
    {
        public SwaggerTagAttribute(string name = null, string description = null, string externalDocUrls = null)
        {
            this.Name = name;

            this.Description = description;

            this.Tag = new Tag { Name = this.Name, Description = this.Description };
            if (externalDocUrls != null)
            {
                this.Tag.ExternalDocs = new ExternalDocs { Url = externalDocUrls };
            }

        }

        public string Name { get; set; }

        public string Description { get; set; }

        public Tag Tag { get; }
    }
}