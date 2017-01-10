using Newtonsoft.Json;
using System.Collections.Generic;

namespace Swashbuckle.AspNetCore.Swagger
{
    public abstract class SecurityScheme
    {
        public SecurityScheme()
        {
            Extensions = new Dictionary<string, object>();
        }

        /// <summary>
        /// The type of the security scheme. Valid values are "basic", "apiKey" or "oauth2".
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// A short description for security scheme.
        /// </summary>
        public string Description { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; private set; }
    }
}
