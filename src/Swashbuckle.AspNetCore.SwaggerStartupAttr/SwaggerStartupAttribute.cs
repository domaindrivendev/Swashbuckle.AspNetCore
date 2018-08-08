using System;

namespace Swashbuckle.AspNetCore.SwaggerStartupAttr
{
    /// <summary>
    /// Marks class as Startup, allowing it to be recognized by --startupattribute CLI option
    /// </summary>
    /// <remarks>
    /// output argument in 'swagger tofile' command must be a folder instead of a fileName
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class ,
                    AllowMultiple = true)
    ]
    public class SwaggerStartupAttribute: System.Attribute
    {
        private string _openApiFileName;
        private string _clientClassName;
        private string _clientNamespace;

        /// <summary>
        /// Name of OpenApi file to be generated
        /// ".swagger.json" is automatically appended to chosen one
        /// </summary>
        public string OpenApiFileName
        {
            get => _openApiFileName;
            set => _openApiFileName = value.Trim() + ".swagger.json";
        }

        /// <summary>
        /// Desired client class name
        /// </summary>
        public string ClientClassName
        {
            get => _clientClassName;
            set => _clientClassName = value.Trim();
        }

        /// <summary>
        /// Desired namespace whithin whom client class would like to be placed
        /// </summary>
        public string ClientNamespace
        {
            get => _clientNamespace;
            set => _clientNamespace = value.Trim();
        }

        public SwaggerStartupAttribute(string openApiFileName)
        {
            OpenApiFileName = openApiFileName;
        }
    }
}
