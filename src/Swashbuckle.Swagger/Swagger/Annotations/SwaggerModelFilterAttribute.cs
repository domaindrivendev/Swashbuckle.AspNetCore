using System;

namespace Swashbuckle.Swagger.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerModelFilterAttribute : Attribute
    {
        public SwaggerModelFilterAttribute(Type filterType)
        {
            FilterType = filterType;
        }

        public Type FilterType { get; private set; }
    }
}