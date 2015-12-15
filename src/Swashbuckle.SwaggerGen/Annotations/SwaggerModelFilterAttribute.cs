using System;

namespace Swashbuckle.SwaggerGen.Annotations
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