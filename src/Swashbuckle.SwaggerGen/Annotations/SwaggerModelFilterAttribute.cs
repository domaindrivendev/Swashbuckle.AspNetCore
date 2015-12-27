using System;

namespace Swashbuckle.SwaggerGen.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerModelFilterAttribute : Attribute
    {
        public SwaggerModelFilterAttribute(Type type)
        {
            Type = type;
            Arguments = new object[]{ };
        }

        public Type Type { get; private set; }

        public object[] Arguments { get; set; }
    }
}