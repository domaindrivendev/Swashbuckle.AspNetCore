using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Swashbuckle.SwaggerUi.Application
{
    public class SwaggerUiConfig
    {
        private readonly Dictionary<string, string> _templateParams;

        public string BaseRoute { get; set; } = "swagger/ui";
        public string SwaggerUrl { get; set; } = "/swagger/v1/swagger.json";

        public SwaggerUiConfig()
        {
            _templateParams = new Dictionary<string, string>
            {
                {"%(StylesheetIncludes)", ""},
                {"%(BooleanValues)", "true|false"},
                {"%(CustomScripts)", ""}
            };
        }

        public string IndexPath => BaseRoute.Trim('/') + "/index.html";

        public void InjectStylesheet(string path, string media = "screen")
        {
            var stringBuilder = new StringBuilder(_templateParams["%(StylesheetIncludes)"]);
            stringBuilder.AppendLine($"<link href='{path}' media='{media}' rel='stylesheet' type='text/css' />");
            _templateParams["%(StylesheetIncludes)"] = stringBuilder.ToString();
        }

        public void InjectJavaScript(string path)
        {
            var stringBuilder = new StringBuilder(_templateParams["%(CustomScripts)"]);
            stringBuilder.AppendLine($"<script src='{path}' type='text/javascript'></script>");
            _templateParams["%(CustomScripts)"] = stringBuilder.ToString();
        }

        public IEnumerable<KeyValuePair<string, string>> GetPlaceholderValues()
        {
            foreach (var pair in _templateParams)
            {
                yield return pair;
            }
            yield return new KeyValuePair<string, string>("%(SwaggerUrl)", SwaggerUrl);
        }
    }
}