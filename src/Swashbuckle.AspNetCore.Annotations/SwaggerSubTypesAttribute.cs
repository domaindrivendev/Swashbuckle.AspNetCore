using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Annotations
{
    [Obsolete("Use multiple SwaggerSubType attributes instead")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false)]
    public class SwaggerSubTypesAttribute : Attribute
    {
        public SwaggerSubTypesAttribute(params Type[] subTypes)
        {
            SubTypes = subTypes;
        }

        public IEnumerable<Type> SubTypes { get; }

        public string Discriminator { get; set; }
    }
}