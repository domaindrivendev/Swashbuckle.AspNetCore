using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.AzureFunctions.Annotations;
using Swashbuckle.AspNetCore.AzureFunctions.Application;

namespace Swashbuckle.AspNetCore.AzureFunctions.Providers
{
    /// <summary>
    /// Api description provider for Azure Functions
    /// </summary>
    internal class FunctionApiDescriptionProvider : IApiDescriptionGroupCollectionProvider
    {
        public ApiDescriptionGroupCollection ApiDescriptionGroups { get; private set; }

        public FunctionApiDescriptionProvider(IOptions<AzureFunctionsOptions> functionsOptions)
        {
            Initialize(functionsOptions.Value.Assembly);
        }

        private void Initialize(Assembly functionsAssembly)
        {
            var methods = functionsAssembly.GetTypes()
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes(typeof(FunctionNameAttribute), false).Any())
                .ToList();

            IList<ApiDescriptionGroup> apiDescrGroup = new List<ApiDescriptionGroup>();
            foreach (MethodInfo methodInfo in methods)
            {
                if (!TryGetHttpTrigger(methodInfo, out var triggerAttribute))
                    continue;

                var functionAttr = (FunctionNameAttribute)methodInfo.GetCustomAttribute(typeof(FunctionNameAttribute), false);
                var route = $"api/{(!string.IsNullOrWhiteSpace(triggerAttribute.Route) ? triggerAttribute.Route : functionAttr.Name)}";
                var verbs = triggerAttribute.Methods ?? new[] { "get", "post", "delete", "head", "patch", "put", "options" };
                var items = new List<ApiDescription>();
                foreach (string verb in verbs)
                {
                    var description = CreateDescription(methodInfo, route, functionAttr, verb);
                    items.Add(description);
                }

                var group = new ApiDescriptionGroup(functionAttr.Name, items);
                apiDescrGroup.Add(group);
            }

            // TODO: is Version necessery? 
            ApiDescriptionGroups = new ApiDescriptionGroupCollection(new ReadOnlyCollection<ApiDescriptionGroup>(apiDescrGroup), 1);
        }

        private static bool TryGetHttpTrigger(MethodInfo methodInfo, out HttpTriggerAttribute triggerAttribute)
        {
            triggerAttribute = null;
            var ignore = methodInfo.GetCustomAttributes().Any(x => x is SwaggerIgnoreAttribute);
            if (ignore)
                return false;

            triggerAttribute = FindHttpTriggerAttribute(methodInfo);
            if (triggerAttribute == null)
                return false;

            return true;
        }

        private static ApiDescription CreateDescription(MethodInfo methodInfo, string route, FunctionNameAttribute functionAttr,
            string verb)
        {
            var description = new ApiDescription()
            {
                ActionDescriptor = new ControllerActionDescriptor()
                {
                    MethodInfo = methodInfo,
                    ControllerName = functionAttr.Name,
                    DisplayName = functionAttr.Name,
                    ControllerTypeInfo = methodInfo.DeclaringType.GetTypeInfo(),
                    Parameters = new List<ParameterDescriptor>()
                },
                RelativePath = route,
                HttpMethod = verb.ToUpper(),
            };

            var supportedMediaTypes = methodInfo.GetCustomAttributes<SupportedMediaTypeAttribute>().Select(x => new ApiRequestFormat() { MediaType = x.MediaType }).ToList();
            foreach (var supportedMediaType in supportedMediaTypes)
            {
                description.SupportedRequestFormats.Add(supportedMediaType);
            }

            var parameters = GetParametersDescription(methodInfo, route);
            parameters.AddRange(GetOptionalQueryParamaters(methodInfo));
            foreach (var parameter in parameters)
            {
                description.ActionDescriptor.Parameters.Add(new ParameterDescriptor()
                {
                    Name = parameter.Name,
                    ParameterType = parameter.Type
                });
                description.ParameterDescriptions.Add(parameter);
            }

            return description;
        }

        private static IEnumerable<ApiParameterDescription> GetOptionalQueryParamaters(MethodInfo methodInfo)
        {
            var optionalQueryParameterAttributes = methodInfo.GetCustomAttributes<OptionalQueryParameterAttribute>();
            foreach (var queryParameterAttribute in optionalQueryParameterAttributes)
            {
                yield return new ApiParameterDescription()
                {
                    Name = queryParameterAttribute.Name,
                    Type = queryParameterAttribute.Type,
                    Source = BindingSource.Query,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        IsOptional = true
                    },
                };
            }
        }

        private static HttpTriggerAttribute FindHttpTriggerAttribute(MethodInfo methodInfo)
        {
            HttpTriggerAttribute triggerAttribute = null;
            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                triggerAttribute = parameter.GetCustomAttributes(typeof(HttpTriggerAttribute), false)
                    .FirstOrDefault() as HttpTriggerAttribute;
                if (triggerAttribute != null)
                    break;
            }

            return triggerAttribute;
        }
        private static List<ApiParameterDescription> GetParametersDescription(MethodInfo methodInfo, string route)
        {
            var parms = new List<ApiParameterDescription>();
            foreach (ParameterInfo parameter in methodInfo.GetParameters())
            {
                var requestBodyTypeAttribute = parameter.GetCustomAttribute(typeof(RequestBodyTypeAttribute)) as RequestBodyTypeAttribute;
                if (parameter.ParameterType == typeof(HttpRequestMessage) && requestBodyTypeAttribute == null)
                    continue;

                if (IgnoreParameter(parameter))
                    continue;

                bool hasHttpTrigerAttribute = parameter.GetCustomAttributes().Any(attr => attr is HttpTriggerAttribute);
                bool hasFromUriAttribute = parameter.GetCustomAttributes().Any(attr => attr is FromUriAttribute);

                var type = hasHttpTrigerAttribute && requestBodyTypeAttribute != null
                    ? requestBodyTypeAttribute.Type
                    : parameter.ParameterType;
                var bindingSource = route.Contains("{" + parameter.Name) ? BindingSource.Path
                    : hasFromUriAttribute ? BindingSource.Query
                    : BindingSource.Body;

                parms.Add(new ApiParameterDescription()
                {
                    Name = parameter.Name,
                    Type = type,
                    Source = bindingSource,
                    RouteInfo = new ApiParameterRouteInfo()
                    {
                        IsOptional = bindingSource == BindingSource.Query
                    },
                });
            }

            return parms;
        }

        private static bool IgnoreParameter(ParameterInfo parameter)
        {
            var ignoreParameterAttribute = parameter.GetCustomAttribute(typeof(SwaggerIgnoreAttribute));
            if (ignoreParameterAttribute != null) return true;
            if (parameter.ParameterType.Name == "TraceWriter") return true;
            if (parameter.ParameterType == typeof(Microsoft.Extensions.Logging.ILogger)) return true;
            if (parameter.ParameterType.IsAssignableFrom(typeof(Microsoft.Extensions.Logging.ILogger))) return true;
            if (parameter.GetCustomAttributes().Any(attr =>
                !(attr is HttpTriggerAttribute)
                && !(attr is FromUriAttribute)
                && !(attr is RequestBodyTypeAttribute)
                && !(attr.GetType().Namespace.Contains("Annotation")))) return true;
            return false;
        }
    }
}

