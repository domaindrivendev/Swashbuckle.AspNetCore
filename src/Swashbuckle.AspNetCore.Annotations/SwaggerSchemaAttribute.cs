using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Parameter |
        AttributeTargets.Property |
        AttributeTargets.Enum,
        AllowMultiple = false)]
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

        public bool Nullable
        {
            get { throw new InvalidOperationException($"Use {nameof(NullableFlag)} instead"); }
            set { NullableFlag = value; }
        }

        public string[] Required { get; set; }

        public string Title { get; set; }

        internal bool? ReadOnlyFlag { get; private set; }

        internal bool? WriteOnlyFlag { get; private set; }

        internal bool? NullableFlag { get; private set; }
    }
}
