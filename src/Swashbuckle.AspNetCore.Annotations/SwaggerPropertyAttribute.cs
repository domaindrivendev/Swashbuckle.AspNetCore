using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    ///     Replace property type output by the real serialized one (in case of use of JsonConverter for instance)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerPropertyAttribute : Attribute
    {
        public Type OverridedType;

        public SwaggerPropertyAttribute(Type overrideType)
        {
            this.OverridedType = overrideType;
        }
    }
}
