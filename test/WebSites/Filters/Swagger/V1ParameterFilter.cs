using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Filters.Swagger
{
    /// <summary>
    /// This filter appends a message to parameter descriptions for the v1 document.
    /// </summary>
    public class V1ParameterFilter : IParameterFilter
    {
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            if (context.DocumentName != "v1")
            {
                return;
            }

            parameter.Description += "<br/><br/><b>This parameter will be removed, please update to the v2 API.</b>";
        }
    }
}
