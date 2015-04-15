using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                ApplyControllerConvention(controller);
            }
        }

        private void ApplyControllerConvention(ControllerModel controller)
        {
            foreach (var action in controller.Actions)
            {
                if (ApiExplorerShouldIgnore(action))
                {
                    action.ApiExplorer.IsVisible = false;
                }
                else
                {
                    action.ApiExplorer.IsVisible = true;
                    action.ApiExplorer.GroupName = controller.ControllerName;
                }
            }
        }

        private bool ApiExplorerShouldIgnore(ActionModel action)
        {
            var actionSettings = action.Attributes
                .OfType<ApiExplorerSettingsAttribute>()
                .FirstOrDefault();
            if (actionSettings != null) return actionSettings.IgnoreApi;

            var controllerSettings = action.Controller.Attributes
                .OfType<ApiExplorerSettingsAttribute>()
                .FirstOrDefault();
            if (controllerSettings != null) return controllerSettings.IgnoreApi;

            return false;
        }
    }
}