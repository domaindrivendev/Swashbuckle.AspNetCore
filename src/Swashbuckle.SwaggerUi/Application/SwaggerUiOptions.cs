using System;
using Swashbuckle.SwaggerUi.CustomAssets;

namespace Swashbuckle.SwaggerUi.Application
{
    public class SwaggerUiOptions
    {
        public string BaseRoute { get; set; } = "swagger";

        public string IndexPath => BaseRoute.Trim('/') + "/index.html";

        internal IndexConfig IndexConfig { get; private set; } = new IndexConfig();

        public void InjectStylesheet(string path, string media = "screen")
        {
            IndexConfig.Stylesheets.Add(new StylesheetDescriptor { Href = path, Media = media });
        }

        public void SwaggerEndpoint(string url, string description)
        {
            IndexConfig.JSConfig.SwaggerEndpoints.Add(new EndpointDescriptor { Url = url, Description = description });
        }

        public void EnabledValidator(string validatorUrl = "http://online.swagger.io/validator")
        {
            IndexConfig.JSConfig.ValidatorUrl = validatorUrl;
        }

        public void BooleanValues(object[] values)
        {
            IndexConfig.JSConfig.BooleanValues = values;
        }

        public void DocExpansion(string value)
        {
            IndexConfig.JSConfig.DocExpansion = value;
        }

        public void SupportedSubmitMethods(string[] supportedSubmitMethods)
        {
            IndexConfig.JSConfig.SupportedSubmitMethods = supportedSubmitMethods;
        }

        [Obsolete("Will be removed in next version, use InjectOnCompleteJavaScript instead")]
        public void InjectJavaScript(string path)
        {
            InjectOnCompleteJavaScript(path);
        }

        public void InjectOnCompleteJavaScript(string path)
        {
            IndexConfig.JSConfig.OnCompleteScripts.Add(path);
        }

        public void InjectOnFailureJavaScript(string path)
        {
            IndexConfig.JSConfig.OnFailureScripts.Add(path);
        }

        public void ShowRequestHeaders()
        {
            IndexConfig.JSConfig.ShowRequestHeaders = true;
        }

        public void ShowJsonEditor()
        {
            IndexConfig.JSConfig.JsonEditor = true;
        }

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
