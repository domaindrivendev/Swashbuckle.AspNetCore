using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.SwaggerGen;
using System.Linq;

namespace Swashbuckle.Application
{
    public class SwaggerProviderBuilder
    {
        private readonly SwaggerProviderOptions _swaggerProviderOptions;
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        private List<FilterTypeOrInstance<IOperationFilter>> _operationFilterDescriptors;
        private List<FilterTypeOrInstance<IDocumentFilter>> _documentFilterDescriptors;
        private List<FilterTypeOrInstance<IModelFilter>> _modelFilterDescriptors;

        private struct FilterTypeOrInstance<TFilter>
        {
            public Type Type;
            public TFilter Instance;
        }

        public SwaggerProviderBuilder()
        {
            _swaggerProviderOptions = new SwaggerProviderOptions();
            _schemaRegistryOptions = new SchemaRegistryOptions();

            _operationFilterDescriptors = new List<FilterTypeOrInstance<IOperationFilter>>();
            _documentFilterDescriptors = new List<FilterTypeOrInstance<IDocumentFilter>>();
            _modelFilterDescriptors = new List<FilterTypeOrInstance<IModelFilter>>();
        }

        public void SingleApiVersion(Info info)
        {
            _swaggerProviderOptions.SingleApiVersion(info);
        }

        public void MultipleApiVersions(
            IEnumerable<Info> apiVersions,
            Func<ApiDescription, string, bool> versionSupportResolver)
        {
            _swaggerProviderOptions.MultipleApiVersions(apiVersions, versionSupportResolver);
        }

        public void IgnoreObsoleteActions()
        {
            _swaggerProviderOptions.IgnoreObsoleteActions = true;
        }

        public void GroupActionsBy(Func<ApiDescription, string> groupNameSelector)
        {
            _swaggerProviderOptions.GroupNameSelector = groupNameSelector;
        }

        public void OrderActionGroupsBy(IComparer<string> groupNameComparer)
        {
            _swaggerProviderOptions.GroupNameComparer = groupNameComparer;
        }

        public void AddSecurityDefinition(string name, SecurityScheme securityScheme)
        {
            _swaggerProviderOptions.SecurityDefinitions.Add(name, securityScheme);
        }

        public void MapType<T>(Func<Schema> schemaFactory)
        {
            _schemaRegistryOptions.CustomTypeMappings.Add(typeof(T), schemaFactory);
        }
        public void DescribeAllEnumsAsStrings()
        {
            _schemaRegistryOptions.DescribeAllEnumsAsStrings = true;
        }

        public void DescribeStringEnumsInCamelCase()
        {
            _schemaRegistryOptions.DescribeStringEnumsInCamelCase = true;
        }

        public void CustomSchemaIds(Func<Type, string> schemaIdSelector)
        {
            _schemaRegistryOptions.SchemaIdSelector = schemaIdSelector; 
        }

        public void IgnoreObsoleteProperties()
        {
            _schemaRegistryOptions.IgnoreObsoleteProperties = true;
        }

        public void OperationFilter(IOperationFilter filter)
        {
            _operationFilterDescriptors.Add(new FilterTypeOrInstance<IOperationFilter> { Instance = filter });
        }

        public void OperationFilter<TFilter>()
            where TFilter : IOperationFilter
        {
            _operationFilterDescriptors.Add(new FilterTypeOrInstance<IOperationFilter> { Type = typeof(TFilter) });
        }

        public void DocumentFilter(IDocumentFilter filter)
        {
            _documentFilterDescriptors.Add(new FilterTypeOrInstance<IDocumentFilter> { Instance = filter });
        }

        public void DocumentFilter<TFilter>()
            where TFilter : IDocumentFilter
        {
            _documentFilterDescriptors.Add(new FilterTypeOrInstance<IDocumentFilter> { Type = typeof(TFilter) });
        }

        public void ModelFilter(IModelFilter filter)
        {
            _modelFilterDescriptors.Add(new FilterTypeOrInstance<IModelFilter> { Instance = filter });
        }

        public void ModelFilter<TFilter>()
            where TFilter : IModelFilter
        {
            _modelFilterDescriptors.Add(new FilterTypeOrInstance<IModelFilter> { Type = typeof(TFilter) });
        }

        public ISwaggerProvider Build(IServiceProvider serviceProvider)
        {
            var schemaRegistryFactory = new SchemaRegistryFactory(
                serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value.SerializerSettings,
                BuildSchemaRegistryOptions(serviceProvider));

            return new SwaggerProvider(
                serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>(),
                schemaRegistryFactory,
                BuildSwaggerProviderOptions(serviceProvider));
        }

        private SchemaRegistryOptions BuildSchemaRegistryOptions(IServiceProvider serviceProvider)
        {
            var options = _schemaRegistryOptions.Clone();

            foreach (var filter in ResolveFilters(_modelFilterDescriptors, serviceProvider))
            {
                options.ModelFilters.Add(filter);
            }

            return options;
        }

        private SwaggerProviderOptions BuildSwaggerProviderOptions(IServiceProvider serviceProvider)
        {
            var options = _swaggerProviderOptions.Clone();

            foreach (var filter in ResolveFilters(_operationFilterDescriptors, serviceProvider))
            {
                options.OperationFilters.Add(filter);
            }

            foreach (var filter in ResolveFilters(_documentFilterDescriptors, serviceProvider))
            {
                options.DocumentFilters.Add(filter);
            }

            return options;
        }

        private IEnumerable<TFilter> ResolveFilters<TFilter>(
            List<FilterTypeOrInstance<TFilter>> _filterDescriptors,
            IServiceProvider serviceProvider)
        {
            return _filterDescriptors.Select(descriptor =>
            {
                if (descriptor.Instance != null) return descriptor.Instance;
                return (TFilter)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, descriptor.Type);
            });
        }
    }
}
