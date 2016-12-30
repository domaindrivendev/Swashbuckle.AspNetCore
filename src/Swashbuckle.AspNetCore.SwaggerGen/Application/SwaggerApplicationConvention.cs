using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerApplicationConvention : IApplicationModelConvention
    {
        public void Apply(ApplicationModel application)
        {
            application.ApiExplorer.IsVisible = true;
        }
    }
}