using Microsoft.AspNet.Mvc.ApplicationModels;
using Swashbuckle.Application;

namespace SampleApi
{
    public class ApiExplorerForSwaggerConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                controller.ApiExplorer.IsVisible = controller.ControllerType != typeof(SwaggerDocsController);
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }
        }
    }
}