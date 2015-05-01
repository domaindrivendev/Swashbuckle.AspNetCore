using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorOptions
    {
        private IList<Info> _apiVersions;

        public SwaggerGeneratorOptions()
        {
            _apiVersions = new List<Info>();
            _apiVersions.Add(new Info { Version = "v1", Title = "API V1" });

            SecurityDefinitions = new Dictionary<string, SecurityScheme>();

            GroupNameSelector = ((apiDesc) => apiDesc.GroupName);
            GroupNameComparer = Comparer<string>.Default;

            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }

        public IEnumerable<Info> ApiVersions
        {
            get { return _apiVersions; }
        }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IList<string> Schemes { get; set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public bool IgnoreObsoleteActions { get; set; }

        public Func<ApiDescription, string> GroupNameSelector { get; private set; }

        public IComparer<string> GroupNameComparer { get; private set; }

        public IList<IOperationFilter> OperationFilters { get; private set; }

        public IList<IDocumentFilter> DocumentFilters { get; private set; }

        public void SingleApiVersion(Info info)
        {
            _apiVersions.Clear();
            _apiVersions.Add(info);
            VersionSupportResolver = null;
        }

        public void MultipleApiVersions(
            IEnumerable<Info> apiVersions,
            Func<ApiDescription, string, bool> versionSupportResolver)
        {
            _apiVersions.Clear();
            foreach (var version in apiVersions)
            {
                _apiVersions.Add(version);
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
    }
}