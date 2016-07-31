using System;

namespace Swashbuckle.SwaggerGen.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SwaggerSchemaFilterAttribute : Attribute
    {
        public SwaggerSchemaFilterAttribute(Type type)
        {
            Type = type;
            Arguments = new object[]{ };
        }

        public Type Type { get; private set; }

        public object[] Arguments { get; set; }
    }
}