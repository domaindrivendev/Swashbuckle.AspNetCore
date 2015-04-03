using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.Mvc.Description;

namespace Swashbuckle.Swagger
{
    public class SwaggerGeneratorOptions
    {
        internal SwaggerGeneratorOptions(
            IReadOnlyDictionary<string, Info> apiVersions = null,
            Func<ApiDescription, string, bool> versionSupportResolver = null,
            IEnumerable<string> schemes = null,
            IDictionary<string, SecurityScheme> securityDefinitions = null,
            bool ignoreObsoleteActions = false,
            Func<ApiDescription, string> groupNameSelector = null,
            IComparer<string> groupNameComparer = null,
            IEnumerable<IOperationFilter> operationFilters = null,
            IEnumerable<IDocumentFilter> documentFilters = null,
            Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver = null)
        {
            ApiVersions = apiVersions ?? DefaultApiVersions();
            VersionSupportResolver = versionSupportResolver;
            Schemes = schemes;
            SecurityDefinitions = securityDefinitions;
            IgnoreObsoleteActions = ignoreObsoleteActions;
            GroupNameSelector = groupNameSelector ?? ((apiDesc) => apiDesc.GroupName);
            GroupNameComparer = groupNameComparer ?? Comparer<string>.Default;
            OperationFilters = operationFilters ?? new List<IOperationFilter>();
            DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
            ConflictingActionsResolver = conflictingActionsResolver ?? DefaultConflictingActionsResolver;
        }

        public IReadOnlyDictionary<string, Info> ApiVersions { get; private set; }

        public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

        public IEnumerable<string> Schemes { get; private set; }

        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

        public bool IgnoreObsoleteActions { get; private set; }

        public Func<ApiDescription, string> GroupNameSelector { get; private set; }

        public IComparer<string> GroupNameComparer { get; private set; }

        public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

        public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

        public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; private set; }

        private IReadOnlyDictionary<string, Info> DefaultApiVersions()
        {
            return new ReadOnlyDictionary<string, Info>(
                new Dictionary<string, Info>
                {
                    { "v1", new Info { version = "v1" , title = "API V1" } }
                });
        }

        private ApiDescription DefaultConflictingActionsResolver(IEnumerable<ApiDescription> apiDescriptions)
        {
            var first = apiDescriptions.First();
            throw new NotSupportedException(string.Format(
                "Not supported by Swagger 2.0: Multiple operations with path '{0}' and method '{1}'. " +
                "See the config setting - \"ResolveConflictingActions\" for a potential workaround",
                first.RelativePathSansQueryString(), first.HttpMethod));
        }
    }
    //{
    //    public SwaggerGeneratorOptions(
    //        Func<ApiDescription, string, bool> versionSupportResolver = null,
    //        IEnumerable<string> schemes = null,
    //        IDictionary<string, SecurityScheme> securityDefinitions = null,
    //        bool ignoreObsoleteActions = false,
    //        Func<ApiDescription, string> groupingKeySelector = null,
    //        IComparer<string> groupingKeyComparer = null,
    //        IDictionary<Type, Func<Schema>> customSchemaMappings = null,
    //        IEnumerable<ISchemaFilter> schemaFilters = null,
    //        bool ignoreObsoleteProperties = false, 
    //        bool useFullTypeNameInSchemaIds = false, 
    //        bool describeAllEnumsAsStrings = false,
    //        IEnumerable<IOperationFilter> operationFilters = null,
    //        IEnumerable<IDocumentFilter> documentFilters = null,
    //        Func<IEnumerable<ApiDescription>, ApiDescription> conflictingActionsResolver = null
    //        )
    //    {
    //        VersionSupportResolver = versionSupportResolver;
    //        Schemes = schemes;
    //        SecurityDefinitions = securityDefinitions;
    //        IgnoreObsoleteActions = ignoreObsoleteActions;
    //        GroupingKeySelector = groupingKeySelector ?? DefaultGroupingKeySelector;
    //        GroupingKeyComparer = groupingKeyComparer ?? Comparer<string>.Default;
    //        CustomSchemaMappings = customSchemaMappings ?? new Dictionary<Type, Func<Schema>>();
    //        SchemaFilters = schemaFilters ?? new List<ISchemaFilter>();
    //        IgnoreObsoleteProperties = ignoreObsoleteProperties;
    //        UseFullTypeNameInSchemaIds = useFullTypeNameInSchemaIds;
    //        DescribeAllEnumsAsStrings = describeAllEnumsAsStrings;
    //        OperationFilters = operationFilters ?? new List<IOperationFilter>();
    //        DocumentFilters = documentFilters ?? new List<IDocumentFilter>();
    //        ConflictingActionsResolver = conflictingActionsResolver ?? DefaultConflictingActionsResolver;
    //    }

    //    public Func<ApiDescription, string, bool> VersionSupportResolver { get; private set; }

    //    public IEnumerable<string> Schemes { get; private set; }

    //    public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }

    //    public bool IgnoreObsoleteActions { get; private set; }

    //    public Func<ApiDescription, string> GroupingKeySelector { get; private set; }

    //    public IComparer<string> GroupingKeyComparer { get; private set; }

    //    public IDictionary<Type, Func<Schema>> CustomSchemaMappings { get; private set; }

    //    public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

    //    public bool IgnoreObsoleteProperties { get; private set; }

    //    public bool UseFullTypeNameInSchemaIds { get; private set; }

    //    public bool DescribeAllEnumsAsStrings { get; private set; }

    //    public IEnumerable<IOperationFilter> OperationFilters { get; private set; }

    //    public IEnumerable<IDocumentFilter> DocumentFilters { get; private set; }

    //    public Func<IEnumerable<ApiDescription>, ApiDescription> ConflictingActionsResolver { get; private set; }

    //    private string DefaultGroupingKeySelector(ApiDescription apiDescription)
    //    {
    //        return apiDescription.GroupName;
    //    }

    //    private ApiDescription DefaultConflictingActionsResolver(IEnumerable<ApiDescription> apiDescriptions)
    //    {
    //        var first = apiDescriptions.First();
    //        throw new NotSupportedException(string.Format(
    //            "Not supported by Swagger 2.0: Multiple operations with path '{0}' and method '{1}'. " +
    //            "See the config setting - \"ResolveConflictingActions\" for a potential workaround",
    //            first.RelativePathSansQueryString(), first.HttpMethod));
    //    }
    //}
}