using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SwaggerResponseAttribute : ProducesResponseTypeAttribute
    {
        public SwaggerResponseAttribute(int statusCode, Type type = null, string description = null)
            : base(type ?? typeof(void), statusCode)
        {
            Description = description;
        }

        public string Description { get; set; }
    }
}
