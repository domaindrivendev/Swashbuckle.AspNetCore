using System.Linq;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        private readonly string _routeTemplate;

        public SwaggerApplicationConvention(string routeTemplate)
        {
            _routeTemplate = routeTemplate;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                ApplyControllerConventions(controller);

                foreach (var action in controller.Actions)
                {
                    ApplyActionConventions(action);
                }
            }
        }

        private void ApplyControllerConventions(ControllerModel controller)
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

        private void ApplyActionConventions(ActionModel action)
        {
            // TODO: Upgrade Mvc - later version will have Properties available here
        }
    }
}