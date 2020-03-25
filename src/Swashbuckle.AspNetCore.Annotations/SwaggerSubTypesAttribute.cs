using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
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