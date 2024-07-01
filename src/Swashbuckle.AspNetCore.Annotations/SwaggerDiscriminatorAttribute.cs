using System;

namespace Swashbuckle.AspNetCore.Annotations
{
#if NET7_0_OR_GREATER
    [Obsolete("Use JsonPolymorphic attribute instead")]
#endif
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public class SwaggerDiscriminatorAttribute : Attribute
    {
        public SwaggerDiscriminatorAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
    }
}