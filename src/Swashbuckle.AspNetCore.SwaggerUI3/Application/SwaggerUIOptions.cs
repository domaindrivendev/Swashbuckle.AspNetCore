using System;

namespace Swashbuckle.AspNetCore.SwaggerUI3
{
    public class SwaggerUI3Options
    {
        /// <summary>
        /// Set a custom route prefix for accessing the swagger-ui.
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        internal IndexSettings IndexSettings { get; private set; } = new IndexSettings();

        /// <summary>
        /// Inject additional CSS stylesheets into the swagger-ui HTML page
        /// </summary>
        /// <param name="path">A path to the stylesheet - i.e. the link "href" attribute</param>
        /// <param name="media">The target media - i.e. the link "media" attribute</param>
        public void InjectStylesheet(string path, string media = "screen")
        {
            IndexSettings.Stylesheets.Add(new StylesheetDescriptor { Href = path, Media = media });
        }

        /// <summary>
        /// Provide one or more Swagger JSON endpoints. Can be fully-qualified or relative to the UI page
        /// </summary>
        /// <param name="url">Can be fully qualified or relative to the current host</param>
        /// <param name="description">The description that appears in the document selector drop-down</param>
        public void SwaggerEndpoint(string url, string description)
        {
            IndexSettings.JSConfig.SwaggerEndpoints.Add(new EndpointDescriptor { Url = url, Description = description });
        }

        /// <summary>
        /// You can use this parameter to set a different validator URL. See swagger-ui project for more info
        /// </summary>
        /// <param name="validatorUrl"></param>
        public void EnabledValidator(string validatorUrl = "https://online.swagger.io/validator")
        {
            IndexSettings.JSConfig.ValidatorUrl = validatorUrl;
        }

        /// <summary>
        /// Change the title of the Swagger UI
        /// </summary>
        /// <param name="value">Title given to Swagger UI</param>
        public void DocumentTitle(string value)
        {
            IndexSettings.DocTitle = value;
        }

        /// <summary>
        /// Controls how the API listing is displayed. See swagger-ui project for more info
        /// </summary>
        /// <param name="value">"none", "list" (default) or "full"</param>
        public void DocExpansion(string value)
        {
            IndexSettings.JSConfig.DocExpansion = value;
        }

        /// <summary>
        /// An array of of the HTTP operations that will have the 'Try it out!' option. See swagger-ui project for more info
        /// </summary>
        /// <param name="supportedSubmitMethods"></param>
        public void SupportedSubmitMethods(params string[] supportedSubmitMethods)
        {
            IndexSettings.JSConfig.SupportedSubmitMethods = supportedSubmitMethods;
        }

        /// <summary>
        /// Inject one or more JavaScripts to invoke when the swagger-ui has successfully loaded
        /// </summary>
        /// <param name="path">A path (fully qualified or relative to the current host) to the script</param>
        public void InjectOnCompleteJavaScript(string path)
        {
            IndexSettings.JSConfig.OnCompleteScripts.Add(path);
        }

        /// <summary>
        /// Inject one or more JavaScripts to invoke if the swagger-ui fails to load
        /// </summary>
        /// <param name="path">A path (fully qualified or relative to the current host) to the script</param>
        public void InjectOnFailureJavaScript(string path)
        {
            IndexSettings.JSConfig.OnFailureScripts.Add(path);
        }

        /// <summary>
        /// Show headers that were sent when making a request via the 'Try it out!' option. See swagger-ui project for more info
        /// </summary>
        public void ShowRequestHeaders()
        {
            IndexSettings.JSConfig.ShowRequestHeaders = true;
        }

        /// <summary>
        /// Enable OAuth2 UI Interactions. See swagger-ui project for more info.
        /// </summary>
        /// <param name="clientId">Default clientId</param>
        /// <param name="clientSecret">Default clientId</param>
        /// <param name="realm">Realm query parameter (for oauth1) added to authorizationUrl and tokenUrl</param>
        /// <param name="appName">Application name, displayed in authorization popup</param>
        /// <param name="scopeSeparator">Scope separator for passing scopes, encoded before calling, default value is a space (encoded value %20)</param>
        /// <param name="additionalQueryStringParameters">Additional query parameters added to authorizationUrl and tokenUrl</param>
        /// <param name="useBasicAuthenticationWithAccessCodeGrant">Only activated for the accessCode flow. During the authorization_code request to the tokenUrl, pass the Client Password using the HTTP Basic Authentication scheme (Authorization header with Basic base64encoded[client_id:client_secret])</param>
        public void ConfigureOAuth2(
            string clientId,
            string clientSecret = null,
            string realm = null,
            string appName = "",
            string scopeSeparator = " ",
            object additionalQueryStringParameters = null,
            bool useBasicAuthenticationWithAccessCodeGrant = false)
        {
            var jsConfig = IndexSettings.JSConfig;
            jsConfig.OAuth2ClientId = clientId;
            jsConfig.OAuth2ClientSecret = clientSecret ?? "na"; //swagger-ui needs a value
            jsConfig.OAuth2Realm = realm ?? "na"; //swagger-ui needs a value
            jsConfig.OAuth2AppName = appName;
            jsConfig.OAuth2ScopeSeparator = scopeSeparator;
            jsConfig.OAuth2AdditionalQueryStringParams = additionalQueryStringParameters ?? new { };
            jsConfig.OAuth2UseBasicAuthenticationWithAccessCodeGrant = useBasicAuthenticationWithAccessCodeGrant;
        }

        /// <summary>
        /// The default expansion depth for models.
        /// </summary>
        /// <param name="number">Expansion depth level</param>
        public void DefaultModelExpandDepth(int number)
        {
            IndexSettings.JSConfig.DefaultModelExpandDepth = number;
        }

        /// <summary>
        /// Controls how models are shown when the API is first rendered
        /// </summary>
        /// <param name="example"></param>
        public void DefaultModelRendering(string example)
        {
            IndexSettings.JSConfig.DefaultModelRendering = example;
        }
    }
}
