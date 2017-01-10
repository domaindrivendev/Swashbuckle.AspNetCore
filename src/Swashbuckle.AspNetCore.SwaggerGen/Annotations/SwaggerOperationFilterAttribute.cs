using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
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