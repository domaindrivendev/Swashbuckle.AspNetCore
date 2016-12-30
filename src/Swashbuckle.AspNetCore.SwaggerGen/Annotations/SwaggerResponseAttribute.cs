using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerResponseAttribute : ProducesResponseTypeAttribute
    {
        public SwaggerResponseAttribute(int statusCode, Type type = null, string description = null)
            : base(type == null ? typeof(void) : type, statusCode)
        {
            this.Description = description;
        }

        public string Description { get; set; }
    }
}
