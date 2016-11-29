using System.Collections.Generic;

namespace Swashbuckle.Swagger.Model
{
    public class OAuth2Scheme : SecurityScheme
    {
        public OAuth2Scheme()
        {
            Type = "oauth2";
        }

        /// <summary>
        /// The flow used by the OAuth2 security scheme. 
        /// Valid values are "implicit", "password", "application" or "accessCode".
        /// </summary>
        public string Flow { get; set; }

        /// <summary>
        /// Valid when <see cref="Flow"/> is "implicit" or "accessCode".
        /// </summary>
        public string AuthorizationUrl { get; set; }

        /// <summary>
        /// Valid when <see cref="Flow"/> is "password", "application", or "accessCode". 
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// The available scopes for the OAuth2 security scheme. Valid when <see cref="Type"/> is oauth2. 
        /// </summary>
        public IDictionary<string, string> Scopes { get; set; }
    }
}
