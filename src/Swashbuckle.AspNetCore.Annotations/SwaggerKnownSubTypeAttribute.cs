using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
    public class SwaggerKnownSubTypeAttribute : Attribute
    {
        public SwaggerKnownSubTypeAttribute(Type subType, string discriminatorValue)
        {
            SubType = subType;
            DiscriminatorValue = discriminatorValue;
        }

        public Type SubType { get; }

        public string DiscriminatorValue { get; }
    }
}
