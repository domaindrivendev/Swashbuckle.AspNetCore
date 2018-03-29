using System;

namespace Swashbuckle.AspNetCore.AzureFunctions.Annotations
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OptionalQueryParameterAttribute : Attribute
    {
        public string Name { get; }
        public Type Type { get; }

        public OptionalQueryParameterAttribute(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}