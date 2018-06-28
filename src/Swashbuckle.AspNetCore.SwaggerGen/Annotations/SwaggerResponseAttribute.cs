using System;
using System.Net;
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

        public SwaggerResponseAttribute(HttpStatusCode statusCode, Type type = null, string description = null) : this((int)statusCode, type, description)
        {
        }

        public string Description { get; set; }
    }
}
