using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNet.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.SwaggerGen.Generator;
using Swashbuckle.SwaggerGen.Annotations;

namespace Swashbuckle.SwaggerGen.Application
{
    public class SwaggerGenOptions
    {
        private readonly SwaggerProviderOptions _swaggerProviderOptions;
        private readonly SchemaRegistryOptions _schemaRegistryOptions;

        private List<FilterDescriptor<IOperationFilter>> _operationFilterDescriptors;
        private List<FilterDescriptor<IDocumentFilter>> _documentFilterDescriptors;
        private List<FilterDescriptor<IModelFilter>> _modelFilterDescriptors;

        private struct FilterDescriptor<TFilter>
        {
            public Type Type;
            public object[] Arguments;
        }

        public SwaggerGenOptions()
        {
            _swaggerProviderOptions = new SwaggerProviderOptions();
            _schemaRegistryOptions = new SchemaRegistryOptions();

            _operationFilterDescriptors = new List<FilterDescriptor<IOperationFilter>>();
            _documentFilterDescriptors = new List<FilterDescriptor<IDocumentFilter>>();
            _modelFilterDescriptors = new List<FilterDescriptor<IModelFilter>>();

            // Enable Annotations
            OperationFilter<SwaggerAttributesOperationFilter>();
            ModelFilter<SwaggerAttributesModelFilter>();
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

        public void OperationFilter<TFilter>(params object[] parameters)
            where TFilter : IOperationFilter
        {
            _operationFilterDescriptors.Add(new FilterDescriptor<IOperationFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void DocumentFilter<TFilter>(params object[] parameters)
            where TFilter : IDocumentFilter
        {
            _documentFilterDescriptors.Add(new FilterDescriptor<IDocumentFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void ModelFilter<TFilter>(params object[] parameters)
            where TFilter : IModelFilter
        {
            _modelFilterDescriptors.Add(new FilterDescriptor<IModelFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void IncludeXmlComments(string xmlDocPath)
        {
            var xmlDoc = new XPathDocument(xmlDocPath);
            OperationFilter<XmlCommentsOperationFilter>(xmlDoc);
            ModelFilter<XmlCommentsModelFilter>(xmlDoc);
        }

        internal SchemaRegistryOptions GetSchemaRegistryOptions(IServiceProvider serviceProvider)
        {
            var options = _schemaRegistryOptions.Clone();

            foreach (var filter in CreateFilters(_modelFilterDescriptors, serviceProvider))
            {
                options.ModelFilters.Add(filter);
            }

            return options;
        }

        internal SwaggerProviderOptions GetSwaggerProviderOptions(IServiceProvider serviceProvider)
        {
            var options = _swaggerProviderOptions.Clone();

            foreach (var filter in CreateFilters(_operationFilterDescriptors, serviceProvider))
            {
                options.OperationFilters.Add(filter);
            }

            foreach (var filter in CreateFilters(_documentFilterDescriptors, serviceProvider))
            {
                options.DocumentFilters.Add(filter);
            }

            return options;
        }

        private IEnumerable<TFilter> CreateFilters<TFilter>(
            List<FilterDescriptor<TFilter>> _filterDescriptors,
            IServiceProvider serviceProvider)
        {
            return _filterDescriptors.Select(descriptor =>
            {
                return (TFilter)ActivatorUtilities.CreateInstance(serviceProvider, descriptor.Type, descriptor.Arguments);
            });
        }
    }
}
