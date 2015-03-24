using System.Linq;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Swagger.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        private readonly string _routeTemplate;

        public SwaggerApplicationConvention(string docsRouteTemplate)
        {
            _routeTemplate = docsRouteTemplate;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType == typeof(SwaggerDocsController))
                {
                    controller.AttributeRoutes.First().Template = _routeTemplate;
                    controller.ApiExplorer.IsVisible = false;
                }
                else
                {
                    controller.ApiExplorer.IsVisible = true;
                    controller.ApiExplorer.GroupName = controller.ControllerName;
                }
            }
        }
    }
}