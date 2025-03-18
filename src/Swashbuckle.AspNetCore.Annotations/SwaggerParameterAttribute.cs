namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Enriches Parameter metadata for "path", "query" or "header" bound parameters or properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class SwaggerParameterAttribute : Attribute
    {
        public SwaggerParameterAttribute(string description = null)
        {
            Description = description;
        }

        /// <summary>
        /// A brief description of the parameter. This could contain examples of use.
        /// GFM syntax can be used for rich text representation
        /// </summary>
        public string Description { get;  set; }

        /// <summary>
        /// Determines whether the parameter is mandatory. If the parameter is in "path",
        /// it will be required by default as Swagger does not allow optional path parameters
        /// </summary>
        public bool Required
        {
            get { throw new InvalidOperationException($"Use {nameof(RequiredFlag)} instead"); }
            set { RequiredFlag = value; }
        }

        internal bool? RequiredFlag { get; set; }
    }
}