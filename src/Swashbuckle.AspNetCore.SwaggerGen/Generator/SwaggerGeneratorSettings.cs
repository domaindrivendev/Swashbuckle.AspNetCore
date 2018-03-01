using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings()
        {
            SwaggerDocs = new Dictionary<string, Info>();
            DocInclusionPredicate = (docName, api) => api.GroupName == null || api.GroupName == docName;
            TagSelector = (apiDesc) => apiDesc.ControllerName();
            SortKeySelector = (apiDesc) => TagSelector(apiDesc);
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IDictionary<string, Info> SwaggerDocs { get; set; }

        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; set; }

        public Func<ApiDescription, string> TagSelector { get; set; }

        public Func<ApiDescription, string> SortKeySelector { get; set; }

        public bool DescribeAllParametersInCamelCase { get; set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public IList<IDictionary<string, IEnumerable<string>>> SecurityRequirements { get; private set; }

        public IList<IOperationFilter> OperationFilters { get; private set; }

        public IList<IDocumentFilter> DocumentFilters { get; private set; }

        internal SwaggerGeneratorSettings Clone()
        {
            return new SwaggerGeneratorSettings
            {
                SwaggerDocs = SwaggerDocs,
                DocInclusionPredicate = DocInclusionPredicate,
                IgnoreObsoleteActions = IgnoreObsoleteActions,
                ConflictingActionsResolver = ConflictingActionsResolver,
                TagSelector = TagSelector,
                SortKeySelector = SortKeySelector,
                DescribeAllParametersInCamelCase = DescribeAllParametersInCamelCase,
                SecurityDefinitions = SecurityDefinitions,
                SecurityRequirements = SecurityRequirements,
                OperationFilters = OperationFilters,
                DocumentFilters = DocumentFilters
            };
        }
    }
}