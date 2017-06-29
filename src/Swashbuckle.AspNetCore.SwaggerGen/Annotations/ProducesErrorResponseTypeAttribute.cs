using System;
using Microsoft.AspNetCore.Mvc;

namespace Swashbuckle.AspNetCore.SwaggerGen.Annotations
{
    public class ProducesErrorResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        public string Description { get; }

        public ProducesErrorResponseTypeAttribute(Type type, int statusCode, string description = "Client Error")
            : base(type, statusCode)
        {
            Description = description;
        }
    }
}