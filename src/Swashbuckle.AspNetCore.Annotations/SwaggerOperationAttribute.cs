using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerOperationAttribute : Attribute
    {
        public SwaggerOperationAttribute(string operationId = null)
        {
            OperationId = operationId;
        }

        public string OperationId { get; set; }

        public string[] Tags { get; set; }

        public string[] Schemes { get; set; }

        public string[] Produces { get; set; }

        public string[] Consumes { get; set; }
    }
}