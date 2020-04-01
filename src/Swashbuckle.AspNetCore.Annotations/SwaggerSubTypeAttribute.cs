using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [Obsolete("Use SwaggerSubTypesAttribute instead")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
    public class SwaggerSubTypeAttribute : Attribute
    {
        public SwaggerSubTypeAttribute(Type subType)
        {
            SubType = subType;
        }

        public Type SubType { get; }
    }
}