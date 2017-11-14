using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Swashbuckle.AspNetCore.SwaggerGen
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

        public static bool IsRequired(this ApiParameterDescription parameterDescription)
        {
            var modelMetadata = parameterDescription.ModelMetadata;
            return (modelMetadata == null) ? false : modelMetadata.IsRequired;
        }
    }
}
