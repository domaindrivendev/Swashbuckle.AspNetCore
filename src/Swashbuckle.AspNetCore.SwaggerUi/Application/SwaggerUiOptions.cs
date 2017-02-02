using System;

namespace Swashbuckle.AspNetCore.SwaggerUi
{
    public class SwaggerUiOptions
    {
        /// <summary>
        /// Set a custom route prefix for accessing the swagger-ui.
        /// </summary>
        public string RoutePrefix { get; set; } = "swagger";

        public string IndexPath => RoutePrefix.Trim('/') + "/index.html";

        internal IndexConfig IndexConfig { get; private set; } = new IndexConfig();

        /// <summary>
        /// Inject additional CSS stylesheets into the swagger-ui HTML page
        /// </summary>
        /// <param name="path">A path to the stylesheet - i.e. the link "href" attribute</param>
        /// <param name="media">The target media - i.e. the link "media" attribute</param>
        public void InjectStylesheet(string path, string media = "screen")
        {
            IndexConfig.Stylesheets.Add(new StylesheetDescriptor { Href = path, Media = media });
        }

        /// <summary>
        /// Provide one or more Swagger JSON endpoints. Can be fully-qualified or relative to the UI page
        /// </summary>
        /// <param name="url">Can be fully qualified or relative to the current host</param>
        /// <param name="description">The description that appears in the document selector drop-down</param>
        public void SwaggerEndpoint(string url, string description)
        {
            IndexConfig.JSConfig.SwaggerEndpoints.Add(new EndpointDescriptor { Url = url, Description = description });
        }

        /// <summary>
        /// You can use this parameter to set a different validator URL. See swagger-ui project for more info
        /// </summary>
        /// <param name="validatorUrl"></param>
        public void EnabledValidator(string validatorUrl = "https://online.swagger.io/validator")
        {
            IndexConfig.JSConfig.ValidatorUrl = validatorUrl;
        }

        /// <summary>
        /// You can use this parameter to change the values in "boolean" dropdowns. See swagger-ui project for more info
        /// </summary>
        /// <param name="values"></param>
        public void BooleanValues(object[] values)
        {
            IndexConfig.JSConfig.BooleanValues = values;
        }

        /// <summary>
        /// Controls how the API listing is displayed. See swagger-ui project for more info
        /// </summary>
        /// <param name="value">"none", "list" (default) or "full"</param>
        public void DocExpansion(string value)
        {
            IndexConfig.JSConfig.DocExpansion = value;
        }

        /// <summary>
        /// An array of of the HTTP operations that will have the 'Try it out!' option. See swagger-ui project for more info
        /// </summary>
        /// <param name="supportedSubmitMethods"></param>
        public void SupportedSubmitMethods(string[] supportedSubmitMethods)
        {
            IndexConfig.JSConfig.SupportedSubmitMethods = supportedSubmitMethods;
        }

        [Obsolete("Will be removed in next version, use InjectOnCompleteJavaScript instead")]
        public void InjectJavaScript(string path)
        {
            InjectOnCompleteJavaScript(path);
        }

        /// <summary>
        /// Inject one or more JavaScripts to invoke when the swagger-ui has successfully loaded
        /// </summary>
        /// <param name="path">A path (fully qualified or relative to the current host) to the script</param>
        public void InjectOnCompleteJavaScript(string path)
        {
            IndexConfig.JSConfig.OnCompleteScripts.Add(path);
        }

        /// <summary>
        /// Inject one or more JavaScripts to invoke if the swagger-ui fails to load
        /// </summary>
        /// <param name="path">A path (fully qualified or relative to the current host) to the script</param>
        public void InjectOnFailureJavaScript(string path)
        {
            IndexConfig.JSConfig.OnFailureScripts.Add(path);
        }

        /// <summary>
        /// Show headers that were sent when making a request via the 'Try it out!' option. See swagger-ui project for more info
        /// </summary>
        public void ShowRequestHeaders()
        {
            IndexConfig.JSConfig.ShowRequestHeaders = true;
        }

        /// <summary>
        /// Enables a graphical view for editing complex bodies. See swagger-ui project for more info
        /// </summary>
        public void ShowJsonEditor()
        {
            IndexConfig.JSConfig.JsonEditor = true;
        }

        /// <summary>
        /// Enable OAuth2 UI Interactions. See swagger-ui project for more info.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="realm"></param>
        /// <param name="appName"></param>
        /// <param name="scopeSeparator"></param>
        /// <param name="additionalQueryStringParameters"></param>
        public void ConfigureOAuth2(
            string clientId,
            string clientSecret,
            string realm,
            string appName,
            string scopeSeparator = " ",
            object additionalQueryStringParameters = null)
        {
            var jsConfig = IndexConfig.JSConfig;
            jsConfig.OAuth2ClientId = clientId;
            jsConfig.OAuth2ClientSecret = clientSecret;
            jsConfig.OAuth2Realm = realm;
            jsConfig.OAuth2AppName = appName;
            jsConfig.OAuth2ScopeSeparator = scopeSeparator;
            jsConfig.OAuth2AdditionalQueryStringParams = additionalQueryStringParameters ?? new { };
        }
    }
}
