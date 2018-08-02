using System;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    /// <summary>
    /// Adds a maximum value to a given property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaximumAttribute : Attribute
    {
        public MaximumAttribute(int maximum)
        {
            Maximum = maximum;
        }

        /// <summary>
        /// The maximum value for the decorated property.
        /// </summary>
        public readonly int Maximum;
    }
}