using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.SwaggerGen.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerProducesResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        public SwaggerProducesResponseTypeAttribute(int statusCode, Type type = null, string description = null)
            : base(type == null ? typeof(void) : type, statusCode)
        {
            this.Description = description;
        }

        public string Description { get; set; }
    }
}
