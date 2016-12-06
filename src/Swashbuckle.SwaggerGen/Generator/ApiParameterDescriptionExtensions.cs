using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.SwaggerGen.Generator
{
    public static class ApiParameterDescriptionExtensions
    {
        public static bool IsPartOfCancellationToken(this ApiParameterDescription parameterDescription)
        {
            if (parameterDescription.Source != BindingSource.ModelBinding) return false;

            var name = parameterDescription.Name;
            return name == "CanBeCanceled"
                || name == "IsCancellationRequested"
                || name.StartsWith("WaitHandle.");
        }
    }
}
