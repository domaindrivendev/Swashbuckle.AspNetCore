using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Adds a minimum value to a given property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MinimumAttribute : Attribute
    {
        public MinimumAttribute(int minimum)
        {
            Minimum = minimum;
        }

        /// <summary>
        /// The minimum value for the decorated property.
        /// </summary>
        public readonly int Minimum;
    }
}