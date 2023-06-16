using System;

namespace Swashbuckle.AspNetCore.Annotations
{
#if NET7_0_OR_GREATER
    [Obsolete("Use JsonDerivedType attribute instead")]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
    public class SwaggerSubTypeAttribute : Attribute
    {
        public SwaggerSubTypeAttribute(Type subType)
        {
            SubType = subType;
        }

        public Type SubType { get; set; }

        public string DiscriminatorValue { get; set; }
    }
}