using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.ApiExplorer;

namespace Swashbuckle.Swagger
{
    public class SwaggerDocumentOptions
    {
        public SwaggerDocumentOptions()
        {
            ApiVersions = new List<Info> { new Info { Version = "v1", Title = "API V1" } };
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            GroupNameSelector = (apiDesc) => apiDesc.GroupName;
            GroupNameComparer = Comparer<string>.Default;
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        internal IList<Info> ApiVersions { get; private set; }

        internal Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public bool IgnoreObsoleteActions { get; set; }

        internal Func<ApiDescription, string> GroupNameSelector { get; private set; }

        internal IComparer<string> GroupNameComparer { get; private set; }

        internal IList<IOperationFilter> OperationFilters { get; private set; }

        internal IList<IDocumentFilter> DocumentFilters { get; private set; }

        public void SingleApiVersion(Info info)
        {
            ApiVersions.Clear();
            ApiVersions.Add(info);
            VersionSupportResolver = null;
        }

        public void MultipleApiVersions(
            IEnumerable<Info> apiVersions,
            Func<ApiDescription, string, bool> versionSupportResolver)
        {
            ApiVersions.Clear();
            foreach (var version in apiVersions)
            {
                ApiVersions.Add(version);
            }
            VersionSupportResolver = versionSupportResolver;
        }

        public void GroupActionsBy(Func<ApiDescription, string> groupNameSelector)
        {
            GroupNameSelector = groupNameSelector;
        }

        public void OrderActionGroupsBy(IComparer<string> groupNameComparer)
        {
            GroupNameComparer = groupNameComparer;
        }

        public void OperationFilter<TFilter>()
            where TFilter : IOperationFilter, new()
        {
            OperationFilter(new TFilter());
        }

        public void OperationFilter<TFilter>(TFilter filter)
            where TFilter : IOperationFilter
        {
            OperationFilters.Add(filter);
        }

        public void DocumentFilter<TFilter>()
            where TFilter : IDocumentFilter, new()
        {
            DocumentFilter(new TFilter());
        }

        public void DocumentFilter<TFilter>(TFilter filter)
            where TFilter : IDocumentFilter
        {
            DocumentFilters.Add(filter);
        }
    }
}