using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger.Annotations
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
    }
}