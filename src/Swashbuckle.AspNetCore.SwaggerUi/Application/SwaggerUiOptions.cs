using System;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class SwaggerUIOptions
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
        /// You can use this parameter to change the values in "boolean" dropdowns. See swagger-ui project for more info
        /// </summary>
        /// <param name="values"></param>
        public void BooleanValues(object[] values)
        {
            IndexSettings.JSConfig.BooleanValues = values;
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
        /// Enables a graphical view for editing complex bodies. See swagger-ui project for more info
        /// </summary>
        public void ShowJsonEditor()
        {
            IndexSettings.JSConfig.JsonEditor = true;
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
            var jsConfig = IndexSettings.JSConfig;
            jsConfig.OAuth2ClientId = clientId;
            jsConfig.OAuth2ClientSecret = clientSecret ?? "na"; //swagger-ui needs a value
            jsConfig.OAuth2Realm = realm ?? "na"; //swagger-ui needs a value
            jsConfig.OAuth2AppName = appName;
            jsConfig.OAuth2ScopeSeparator = scopeSeparator;
            jsConfig.OAuth2AdditionalQueryStringParams = additionalQueryStringParameters ?? new { };
        }
    }
}
