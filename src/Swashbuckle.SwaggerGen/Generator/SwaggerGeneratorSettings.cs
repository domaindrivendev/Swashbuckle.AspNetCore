using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.Swagger.Model;

namespace Swashbuckle.SwaggerGen.Generator
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings()
        {
            SwaggerDocs = new Dictionary<string, SwaggerDocumentDescriptor>();
            TagSelector = (apiDesc) => apiDesc.ControllerName();
            TagComparer = Comparer<string>.Default;
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IDictionary<string, SwaggerDocumentDescriptor> SwaggerDocs { get; private set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<ApiDescription, string> TagSelector { get; set; }

        public IComparer<string> TagComparer { get; set; }

        public bool DescribeAllParametersInCamelCase { get; set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public IList<IOperationFilter> OperationFilters { get; private set; }

        public IList<IDocumentFilter> DocumentFilters { get; private set; }

        internal SwaggerGeneratorSettings Clone()
        {
            return new SwaggerGeneratorSettings
            {
                SwaggerDocs = SwaggerDocs,
                IgnoreObsoleteActions = IgnoreObsoleteActions,
                TagSelector = TagSelector,
                TagComparer = TagComparer,
                SecurityDefinitions = SecurityDefinitions,
                OperationFilters = OperationFilters,
                DocumentFilters = DocumentFilters
            };
        }
    }

    public class SwaggerDocumentDescriptor
    {
        public SwaggerDocumentDescriptor(Info info, Func<ApiDescription, bool> includeActionPredicate = null)
        {
            Info = info;
            IncludeActionPredicate = includeActionPredicate ?? ((apiDesc) => true); // includes all actions by default
        }

        public Info Info { get; private set; }

        public Func<ApiDescription, bool> IncludeActionPredicate { get; private set; }
    }
}