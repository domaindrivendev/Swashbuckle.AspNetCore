using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace Swashbuckle.SwaggerUi.CustomAssets
{
    public class IndexConfig
    {
        public IList<StylesheetDescriptor> Stylesheets { get; private set; } = new List<StylesheetDescriptor>();

        public JSConfig JSConfig { get; private set; } = new JSConfig();

        public IDictionary<string, string> ToParamDictionary()
        {
            return new Dictionary<string, string>
            {
                { "%(StylesheetsHtml)", GetStylesheetsHtml() },
                { "%(JSConfig)", JsonConvert.SerializeObject(JSConfig) }
            };
        }

        private string GetStylesheetsHtml()
        {
            var builder = new StringBuilder();
            foreach (var ss in Stylesheets)
            {
                builder.AppendLine($"<link href='{ss.Href}' rel='stylesheet' media='{ss.Media}' type='text/css' />");
            }
            return builder.ToString();
        }
    }

    public class StylesheetDescriptor
    {
        public string Href { get; set; }

        public string Media { get; set; }
    }

    public class JSConfig
    {
        public IList<EndpointDescriptor> SwaggerEndpoints { get; private set; } = new List<EndpointDescriptor>();

        public string ValidatorUrl { get; set; } = null;

        public object[] BooleanValues { get; set; } = new[] { "false", "true" };

        public string DocExpansion { get; set; } = "list";

        public string[] SupportedSubmitMethods { get; set; }  = new[] { "get", "post", "put", "delete", "patch" };

        public IList<string> OnCompleteScripts { get; private set; } = new List<string>();

        public IList<string> OnFailureScripts { get; private set; } = new List<string>();

        public bool ShowRequestHeaders { get; set; } = false;

        public bool JsonEditor { get; set; } = false;

        public string OAuth2ClientId { get; set; } = "your-client-id";

        public string OAuth2ClientSecret { get; set; } = "your-client-secret-if-required";

        public string OAuth2Realm { get; set; } = "your-realms";

        public string OAuth2AppName { get; set; } = "your-app-name";

        public string OAuth2ScopeSeparator { get; set; } = " ";

        public object OAuth2AdditionalQueryStringParams { get; set; } = new { };
    }

    public class EndpointDescriptor
    {
        public string Url { get; set; }

        public string Description { get; set; }
    }
}