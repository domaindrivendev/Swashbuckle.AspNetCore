using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    
    public class SwaggerDescriptionAttribute : Attribute
    {
        public SwaggerDescriptionAttribute(string description = null)
        {
            this.Description = description;
        }

        public string Description { get; set; }
    }
}
