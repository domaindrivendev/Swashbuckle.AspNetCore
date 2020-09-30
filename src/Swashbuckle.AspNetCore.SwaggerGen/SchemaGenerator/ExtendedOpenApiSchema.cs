using System;
using System.Reflection;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class ExtendedOpenApiSchema : OpenApiSchema
    {
        public Func<object, string> EnumConverter { get; set; }
    }
}
