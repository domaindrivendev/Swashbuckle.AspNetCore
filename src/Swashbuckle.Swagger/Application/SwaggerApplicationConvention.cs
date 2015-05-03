using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Mvc.ApplicationModels;

namespace Swashbuckle.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
            foreach (var controller in application.Controllers)
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;

                foreach (var action in controller.Actions)
                {
                    AddProperties(action, controller.Attributes);
                }
            }
        }

        private void AddProperties(ActionModel action, IEnumerable<object> controllerAttributes)
        {
            action.Properties.Add("ControllerAttributes", controllerAttributes.ToArray());
            action.Properties.Add("ActionAttributes", action.Attributes.ToArray());
            action.Properties.Add("IsObsolete", action.Attributes.OfType<ObsoleteAttribute>().Any());
        }
    }
}