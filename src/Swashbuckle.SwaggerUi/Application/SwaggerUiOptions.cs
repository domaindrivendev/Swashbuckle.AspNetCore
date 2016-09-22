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

        public void InjectJavaScript(string path)
        {
            IndexConfig.JSConfig.OnCompleteScripts.Add(path);
        }
    }
}
