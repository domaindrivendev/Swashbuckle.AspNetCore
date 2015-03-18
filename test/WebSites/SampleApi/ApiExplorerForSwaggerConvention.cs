using Microsoft.AspNet.Mvc.ApplicationModels;
using SampleApi.Controllers;
using Swashbuckle.Swagger.Application;

namespace SampleApi
{
    public class ApiExplorerForSwaggerConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                controller.ApiExplorer.IsVisible = controller.ControllerType == typeof(JsonAnnotationsController);
                    //controller.ControllerType != typeof(SwaggerDocsController);
                controller.ApiExplorer.GroupName = controller.ControllerName;
            }
        }
    }
}