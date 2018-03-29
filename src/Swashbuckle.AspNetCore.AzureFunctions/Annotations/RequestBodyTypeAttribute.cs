using System;
using System.Net.Http;

namespace Swashbuckle.AspNetCore.AzureFunctions.Annotations
{
    /// <summary>
    /// Explicite body type definition for functions with <see cref="HttpRequestMessage"/> as input parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestBodyTypeAttribute : Attribute
    {
        /// <summary>
        /// Body model type
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Body model description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Explicite body type definition for functions with <see cref="HttpRequestMessage"/> as input parameter
        /// </summary>
        /// <param name="bodyType">Body model type</param>
        /// <param name="description">Model description</param>
        public RequestBodyTypeAttribute(Type bodyType, string description)
        {
            Type = bodyType;
            Description = description;
        }
    }
}