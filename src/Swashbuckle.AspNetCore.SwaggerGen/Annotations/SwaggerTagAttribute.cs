using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class SwaggerTagAttribute : Attribute
    {
        public SwaggerTagAttribute(string name = null, string description = null)
        {
            this.Tag = new Tag { Name = name, Description = description };
            this.Name = name;
            this.Description = description;
        }

        public SwaggerTagAttribute(Tag tag)
        {
            this.Tag = tag;
            this.Name = tag.Name;
            this.Description = tag.Description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public Tag Tag { get; set; }
    }
}
