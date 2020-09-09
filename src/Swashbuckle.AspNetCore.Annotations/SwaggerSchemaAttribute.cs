using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerSchemaAttribute : Attribute
    {
        public SwaggerSchemaAttribute(string description = null)
        {
            Description = description;
        }

        public string Description { get; set; }

        public string Format { get; set; }

        public bool ReadOnly
        {
            get { throw new InvalidOperationException($"Use {nameof(ReadOnlyFlag)} instead"); }
            set { ReadOnlyFlag = value; }
        }

        public bool WriteOnly
        {
            get { throw new InvalidOperationException($"Use {nameof(WriteOnlyFlag)} instead"); }
            set { WriteOnlyFlag = value; }
        }

        public string[] Required { get; set; }

        public string Title { get; set; }

        internal bool? ReadOnlyFlag { get; private set; }

        internal bool? WriteOnlyFlag { get; private set; }
    }
}
