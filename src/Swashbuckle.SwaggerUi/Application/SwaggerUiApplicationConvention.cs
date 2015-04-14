using System.Linq;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Application
{
    public class SwaggerUiApplicationConvention : IApplicationModelConvention
    {
        private readonly string _customRoute;

        public SwaggerUiApplicationConvention(string customRoute)
        {
            _customRoute = customRoute;
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                if (controller.ControllerType == typeof(SwaggerUiController))
                {
                    ApplySwaggerUiControllerConvention(controller);
                    break;
                }
            }
        }

        private void ApplySwaggerUiControllerConvention(ControllerModel controller)
        {
            controller.ApiExplorer.IsVisible = false;

            if (_customRoute != null)
                controller.Actions.First().AttributeRouteModel.Template = _customRoute;
        }
    }
}