using System.Collections.Generic;

namespace Swashbuckle.SwaggerUi.Application
{
    public class JSConfig
    {
        public IList<SwaggerEndpoint> swaggerEndpoints = new List<SwaggerEndpoint>();

        public IList<string> onCompleteScripts = new List<string>();
    }

    public class SwaggerEndpoint
    {
        public string url;
        public string description;
    }
}
