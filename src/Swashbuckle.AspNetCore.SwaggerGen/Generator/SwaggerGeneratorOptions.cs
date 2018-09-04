using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGeneratorOptions
    {
        public SwaggerGeneratorOptions()
        {
            SwaggerDocs = new Dictionary<string, Info>();
            DocInclusionPredicate = DefaultDocInclusionPredicate;
            OperationIdSelector = DefaultOperationIdSelector;
            TagsSelector = DefaultTagsSelector;
            SortKeySelector = DefaultSortKeySelector;
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>();
            ParameterFilters = new List<IParameterFilter>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IDictionary<string, Info> SwaggerDocs { get; set; }

        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; set; }

        public Func<ApiDescription, string> OperationIdSelector { get; set; }

        public Func<ApiDescription, IList<string>> TagsSelector { get; set; }

        public Func<ApiDescription, string> SortKeySelector { get; set; }

        public bool DescribeAllParametersInCamelCase { get; set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; set; }

        public IList<IDictionary<string, IEnumerable<string>>> SecurityRequirements { get; set; }

        public IList<IParameterFilter> ParameterFilters { get; set; }

        public List<IOperationFilter> OperationFilters { get; set; }

        public IList<IDocumentFilter> DocumentFilters { get; set; }

        private bool DefaultDocInclusionPredicate(string documentName, ApiDescription apiDescription)
        {
            return apiDescription.GroupName == null || apiDescription.GroupName == documentName;
        }

        private string DefaultOperationIdSelector(ApiDescription apiDescription)
        {
            var routeName = apiDescription.ActionDescriptor.AttributeRouteInfo?.Name;
            if (routeName != null) return routeName;

            if (apiDescription.TryGetMethodInfo(out MethodInfo methodInfo)) return methodInfo.Name;

            return null;
        }

        private IList<string> DefaultTagsSelector(ApiDescription apiDescription)
        {
            return new[] { apiDescription.ActionDescriptor.RouteValues["controller"] };
        }

        private string DefaultSortKeySelector(ApiDescription apiDescription)
        {
            return TagsSelector(apiDescription).First();
        }
    }
}