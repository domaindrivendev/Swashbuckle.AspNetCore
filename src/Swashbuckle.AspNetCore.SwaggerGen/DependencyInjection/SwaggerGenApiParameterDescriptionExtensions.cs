using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.ApiDescriptions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenApiParameterDescriptionExtensions
    {
        public static bool IsRequired(
            this ApiParameterDescription apiParameter)
        {
            return (apiParameter.IsFromPath())
                   || apiParameter.CustomAttributes().Any(attr => attr is BindRequiredAttribute || attr is RequiredAttribute);
        }
    }
}
