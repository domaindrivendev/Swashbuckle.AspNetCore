using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.SwaggerStartupAttr
{
    static public class SwaggerStartupAttributeHelpers
    {
        static public IEnumerable<Type> GetClassesWithSwaggerStartupAttribute(this System.Reflection.Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsDefined(typeof(SwaggerStartupAttribute), true))
                {
                    yield return type;
                }
            }
        }

        static public SwaggerStartupAttribute GetSwaggerStartupAttribute(this Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(SwaggerStartupAttribute)) as SwaggerStartupAttribute;
        }
    }
}
