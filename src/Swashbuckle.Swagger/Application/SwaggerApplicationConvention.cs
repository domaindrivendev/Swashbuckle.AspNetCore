using System;
using System.Linq;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        private readonly string _customRoute;

        public SwaggerApplicationConvention(string customRoute)
        {
            _customRoute = customRoute;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType == typeof(SwaggerDocsController))
                {
                    ApplySwaggerDocsControllerConvention(controller);
                }
                else
                {
                    ApplyAppControllerConvention(controller);
                }
            }
        }

        private void ApplySwaggerDocsControllerConvention(ControllerModel controller)
        {
            controller.ApiExplorer.IsVisible = false;

            if (_customRoute != null)
                controller.Actions.First().AttributeRouteModel.Template = _customRoute;
        }

        private void ApplyAppControllerConvention(ControllerModel controller)
        {
            controller.ApiExplorer.IsVisible = true;
            controller.ApiExplorer.GroupName = controller.ControllerName;
        }
    }
}