using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Swashbuckle.AspNetCore.SwaggerUI
{
    public class IndexSettings
    {
        public IList<StylesheetDescriptor> Stylesheets { get; private set; } = new List<StylesheetDescriptor>();

        public string DocTitle { get; set; } = "Swagger UI";

        public JSConfig JSConfig { get; private set; } = new JSConfig();

        public IDictionary<string, string> ToTemplateParameters()
        {
            if (!JSConfig.SwaggerEndpoints.Any())
                throw new InvalidOperationException(
                    "Swagger endpoint(s) not specified. " +
                    "One or more Swagger JSON URL's must be provided to use the swagger-ui."
                );

            return new Dictionary<string, string>
            {
                { "%(DocumentTitle)", DocTitle },
                { "%(StylesheetsHtml)", GetStylesheetsHtml() },
                { "%(JSConfig)", GetJSConfigJson() }
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

        private string GetJSConfigJson()
        {
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver()
            };

            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, JSConfig);
                return writer.ToString();
            }
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