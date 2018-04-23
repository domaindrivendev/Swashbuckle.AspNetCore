using System;

namespace Swashbuckle.AspNetCore.Annotations
{
    /// <summary>
    /// Overrides/defines operation specific attributes.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerOperationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerOperationAttribute"/> class.
        /// </summary>
        /// <param name="operationId">The operation identifier.</param>
        public SwaggerOperationAttribute(string operationId = null)
        {
            OperationId = operationId;
        }

        /// <summary>
        /// Override the OperationId otherwise set by the generator. Note that the OerationId should be unqiue.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Set the tags for the operation. Use this to override tags otherwise set by the generator.
        /// </summary>
        public string[] Tags { get; set; }

        /// <summary>
        /// Sets the schemes for the operation. Use this to override the generator.
        /// </summary>
        /// <remarks>
        /// Consider using this if for instance you want to support HTTP and HTTPS but want to only "advertise" HTTPS.
        /// </remarks>
        public string[] Schemes { get; set; }

        /// <summary>
        /// Sets the content types produced by this operation. Use this to override the generator.
        /// </summary>
        /// <remarks>
        /// You will typically set these when you want to convey information but don't want to actually disable the Mvc output formatters.
        /// </remarks>
        public string[] Produces { get; set; }

        /// <summary>
        /// Sets the content types consumed by this operation. Use this to override the generator.
        /// </summary>
        /// <remarks>
        /// You will typically set these when you want to convey information but don't want to actually disable the Mvc input formatters.
        /// </remarks>
        public string[] Consumes { get; set; }
    }
}