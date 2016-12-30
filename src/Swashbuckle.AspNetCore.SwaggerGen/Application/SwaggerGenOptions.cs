using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace Swashbuckle.AspNetCore.SwaggerGen
{
    public class SwaggerGenOptions
    {
        private readonly SwaggerGeneratorSettings _swaggerGeneratorSettings;
        private readonly SchemaRegistrySettings _schemaRegistrySettings;

        private IList<Func<XPathDocument>> _xmlDocFactories;
        private List<FilterDescriptor<IOperationFilter>> _operationFilterDescriptors;
        private List<FilterDescriptor<IDocumentFilter>> _documentFilterDescriptors;
        private List<FilterDescriptor<ISchemaFilter>> _schemaFilterDescriptors;

        private struct FilterDescriptor<TFilter>
        {
            public Type Type;
            public object[] Arguments;
        }

        public SwaggerGenOptions()
        {
            _swaggerGeneratorSettings = new SwaggerGeneratorSettings();
            _schemaRegistrySettings = new SchemaRegistrySettings();

            _xmlDocFactories = new List<Func<XPathDocument>>();
            _operationFilterDescriptors = new List<FilterDescriptor<IOperationFilter>>();
            _documentFilterDescriptors = new List<FilterDescriptor<IDocumentFilter>>();
            _schemaFilterDescriptors = new List<FilterDescriptor<ISchemaFilter>>();

            // Enable Annotations
            OperationFilter<SwaggerAttributesOperationFilter>();
            OperationFilter<SwaggerResponseAttributeFilter>();
            SchemaFilter<SwaggerAttributesSchemaFilter>();
        }

        public void SwaggerDoc(string name, Info info)
        {
            _swaggerGeneratorSettings.SwaggerDocs.Add(name, info);
        }

        public void DocInclusionPredicate(Func<string, ApiDescription, bool> predicate)
        {
            _swaggerGeneratorSettings.DocInclusionPredicate = predicate;
        }

        public void IgnoreObsoleteActions()
        {
            _swaggerGeneratorSettings.IgnoreObsoleteActions = true;
        }

        public void TagActionsBy(Func<ApiDescription, string> tagSelector)
        {
            _swaggerGeneratorSettings.TagSelector = tagSelector;
        }

        public void OrderActionsBy(Func<ApiDescription, string> sortKeySelector)
        {
            _swaggerGeneratorSettings.SortKeySelector = sortKeySelector;
        }

        public void AddSecurityDefinition(string name, SecurityScheme securityScheme)
        {
            _swaggerGeneratorSettings.SecurityDefinitions.Add(name, securityScheme);
        }

        public void MapType(Type type, Func<Schema> schemaFactory)
        {
            _schemaRegistrySettings.CustomTypeMappings.Add(type, schemaFactory);
        }

        public void MapType<T>(Func<Schema> schemaFactory)
        {
            MapType(typeof(T), schemaFactory);
        }

        public void DescribeAllEnumsAsStrings()
        {
            _schemaRegistrySettings.DescribeAllEnumsAsStrings = true;
        }

        public void DescribeStringEnumsInCamelCase()
        {
            _schemaRegistrySettings.DescribeStringEnumsInCamelCase = true;
        }

        public void CustomSchemaIds(Func<Type, string> schemaIdSelector)
        {
            _schemaRegistrySettings.SchemaIdSelector = schemaIdSelector;
        }

        public void IgnoreObsoleteProperties()
        {
            _schemaRegistrySettings.IgnoreObsoleteProperties = true;
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

        public void SchemaFilter<TFilter>(params object[] parameters)
            where TFilter : ISchemaFilter
        {
            _schemaFilterDescriptors.Add(new FilterDescriptor<ISchemaFilter>
            {
                Type = typeof(TFilter),
                Arguments = parameters
            });
        }

        public void IncludeXmlComments(Func<XPathDocument> xmlDocFactory)
        {
            _xmlDocFactories.Add(xmlDocFactory);
        }

        public void IncludeXmlComments(string filePath)
        {
            IncludeXmlComments(() => new XPathDocument(filePath));
        }

        internal ISwaggerProvider CreateSwaggerProvider(IServiceProvider serviceProvider)
        {
            var swaggerGeneratorSettings = CreateSwaggerGeneratorSettings(serviceProvider);
            var schemaRegistrySettings = CreateSchemaRegistrySettings(serviceProvider);

            // Instantiate & add the XML comments filters here so they're executed before any custom
            // filters AND so they can share the same XPathDocument (perf. optimization)
            foreach (var xmlDocFactory in _xmlDocFactories)
            {
                var xmlDoc = xmlDocFactory();
                swaggerGeneratorSettings.OperationFilters.Insert(0, new XmlCommentsOperationFilter(xmlDoc));
                schemaRegistrySettings.SchemaFilters.Insert(0, new XmlCommentsSchemaFilter(xmlDoc));
            }

            var schemaRegistryFactory = new SchemaRegistryFactory(
                serviceProvider.GetRequiredService<IOptions<MvcJsonOptions>>().Value.SerializerSettings,
                schemaRegistrySettings
            );

            return new SwaggerGenerator(
                serviceProvider.GetRequiredService<IApiDescriptionGroupCollectionProvider>(),
                schemaRegistryFactory,
                swaggerGeneratorSettings
            );
        }

        private SchemaRegistrySettings CreateSchemaRegistrySettings(IServiceProvider serviceProvider)
        {
            var settings = _schemaRegistrySettings.Clone();

            foreach (var filter in CreateFilters(_schemaFilterDescriptors, serviceProvider))
            {
                settings.SchemaFilters.Add(filter);
            }

            return settings;
        }

        private SwaggerGeneratorSettings CreateSwaggerGeneratorSettings(IServiceProvider serviceProvider)
        {
            var settings = _swaggerGeneratorSettings.Clone();

            foreach (var filter in CreateFilters(_operationFilterDescriptors, serviceProvider))
            {
                settings.OperationFilters.Add(filter);
            }

            foreach (var filter in CreateFilters(_documentFilterDescriptors, serviceProvider))
            {
                settings.DocumentFilters.Add(filter);
            }

            return settings;
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