using System;

namespace Swashbuckle.AspNetCore.StartupAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,
                    AllowMultiple = true)
    ]
    public class StartupClassAttribute: System.Attribute
    {
        public string JsonName { get; }

        public StartupClassAttribute(string jsonName)
        {
            JsonName = jsonName;
        }
    }
}
