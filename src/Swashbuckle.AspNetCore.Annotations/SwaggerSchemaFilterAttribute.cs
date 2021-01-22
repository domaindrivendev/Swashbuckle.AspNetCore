using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Enum,
        AllowMultiple = false)]
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