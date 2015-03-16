using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public static class ApiParameterDescriptionExtensions
    {
        public static bool IsRequired(this ApiParameterDescription paramDesc)
        {
            if (paramDesc.RouteInfo != null)
                return !paramDesc.RouteInfo.IsOptional;

            return paramDesc.ModelMetadata.IsRequired;
        }
    }
}