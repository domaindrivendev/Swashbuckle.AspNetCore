using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public class SwaggerSubTypeAttribute : Attribute
    {
        public SwaggerSubTypeAttribute(Type subType)
        {
            SubType = subType;
        }

        public Type SubType { get; }
    }
}