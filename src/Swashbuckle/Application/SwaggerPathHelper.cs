using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.OptionsModel;

namespace Swashbuckle.Application
{
    public class SwaggerPathHelper
    {
        private readonly IOptions<SwaggerOptions> _optionsAccessor;
        private string _routeTemplate;

        public SwaggerPathHelper(IOptions<SwaggerOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public void SetRouteTemplate(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
        }
        
        public IEnumerable<string> GetLocalPaths()
        {
            var swaggerOptions = _optionsAccessor.Options;
            if (swaggerOptions == null || _routeTemplate == null) return Enumerable.Empty<string>();

            return swaggerOptions.SwaggerGeneratorOptions.ApiVersions
                .Select(info => _routeTemplate.Replace("{apiVersion}", info.Version));
        }
    }
}