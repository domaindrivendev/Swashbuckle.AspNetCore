using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public static class ApiParameterDescriptionExtensions
    {
        public static bool IsRequired(this ApiParameterDescription paramDesc)
        {
            return (paramDesc.RouteInfo != null)
                ? !paramDesc.RouteInfo.IsOptional
                : false;
        }
    }
}