using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Swashbuckle.SwaggerGen.Application
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
            foreach (var controller in application.Controllers)
            {
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }
        }
    }
}