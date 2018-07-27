using System;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.StartupAttribute
{
    static public class StartupAttributeHelpers
    {
        static public IEnumerable<Type> GetClassesWithStartupAttribute(this System.Reflection.Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsDefined(typeof(StartupAttribute), true))
                {
                    yield return type;
                }
            }
        }
    }
}
