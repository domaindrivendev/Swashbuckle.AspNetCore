using System;

namespace Swashbuckle.Swagger.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class SwaggerOperationFilterAttribute : Attribute
    {
        public SwaggerOperationFilterAttribute(Type filterType)
        {
            FilterType = filterType;
        }

        public Type FilterType { get; private set; }
    }
}