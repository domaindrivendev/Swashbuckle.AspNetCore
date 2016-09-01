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

        public IList<string> OnCompleteScripts { get; private set; } = new List<string>();
    }

    public class EndpointDescriptor
    {
        public string Url { get; set; }

        public string Description { get; set; }
    }
}