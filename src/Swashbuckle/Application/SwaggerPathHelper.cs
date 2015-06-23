using System;
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
        
        public IEnumerable<string> GetPaths()
        {
            var swaggerOptions = _optionsAccessor.Options;
            if (swaggerOptions == null) return Enumerable.Empty<string>();

            if (_routeTemplate == null)
                throw new InvalidOperationException("Failed to build Swagger Paths - route template not set");

            return swaggerOptions.SwaggerGeneratorOptions.ApiVersions
                .Select(info => _routeTemplate.Replace("{apiVersion}", info.Version));
        }
    }
}