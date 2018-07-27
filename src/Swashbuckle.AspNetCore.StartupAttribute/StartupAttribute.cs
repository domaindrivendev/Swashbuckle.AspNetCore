using System;

namespace Swashbuckle.AspNetCore.StartupAttribute
{
    [AttributeUsage(AttributeTargets.Class
                    | AttributeTargets.Struct,
                    AllowMultiple = true)  // multiuse attribute
    ]
    public class StartupAttribute : Attribute
    {
        public string Name { get; } // public or private?

        public StartupAttribute(string name)
        {
            Name = name;
        }
    }
}
