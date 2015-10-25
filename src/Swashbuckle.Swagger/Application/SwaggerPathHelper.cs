using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Framework.OptionsModel;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public class SwaggerPathHelper
    {
        private readonly IOptions<SwaggerDocumentOptions> _optionsAccessor;
        private string _routeTemplate;

        public SwaggerPathHelper(IOptions<SwaggerDocumentOptions> optionsAccessor)
        {
            _optionsAccessor = optionsAccessor;
        }

        public void SetRouteTemplate(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
        }
        
        public IEnumerable<SwaggerPathDescriptor> GetPathDescriptors(string basePath)
        {
            return _optionsAccessor.Value.ApiVersions
                .Select((info) => CreatePathDescriptor(info, basePath));
        }

        private SwaggerPathDescriptor CreatePathDescriptor(Info info, string basePath)
        {
            var pathBuilder = new StringBuilder("/" + _routeTemplate.Replace("{apiVersion}", info.Version));
            if (basePath != null)
                pathBuilder.Insert(0, basePath);

            return new SwaggerPathDescriptor { Path = pathBuilder.ToString(), Info = info };
        }
    }

    public class SwaggerPathDescriptor
    {
        public string Path { get; set; }

        public Info Info { get; set; }
    }
}