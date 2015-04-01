using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Swashbuckle.Swagger.Generator;

namespace Swashbuckle.Swagger.Application
{
    public class SchemaGeneratorOptionsBuilder
    {
        private IDictionary<Type, Func<Schema>> _customTypeMappings;
        private IList<Func<IModelFilter>> _modelFilters;
        private bool _ignoreObsoleteProperties;
        private bool _useFullTypeNameInSchemaIds;
        private bool _describeAllEnumsAsStrings;
        private string _schemaReferencePrefix;

        public SchemaGeneratorOptionsBuilder()
        {
            _customTypeMappings = new Dictionary<Type, Func<Schema>>();
            _modelFilters = new List<Func<IModelFilter>>();
        }

        public void MapType<T>(Func<Schema> createSchema)
        {
            _customTypeMappings[typeof(T)] = createSchema;
        }

        public void ModelFilter<TFilter>()
            where TFilter : IModelFilter, new()
        {
            ModelFilter(() => new TFilter());
        }

        public void ModelFilter(Func<IModelFilter> createFilter)
        {
            _modelFilters.Add(createFilter);
        }

        public void IgnoreObsoleteProperties()
        {
            _ignoreObsoleteProperties = true;
        }

        public void UseFullTypeNameInSchemaIds()
        {
            _useFullTypeNameInSchemaIds = true;
        }

        public void DescribeAllEnumsAsStrings()
        {
            _describeAllEnumsAsStrings = true;
        }

        public SchemaGeneratorOptions Build()
        {
            var modelFilters = _modelFilters.Select(factory => factory());

            return new SchemaGeneratorOptions(
                customTypeMappings: new ReadOnlyDictionary<Type, Func<Schema>>(_customTypeMappings),
                modelFilters: modelFilters,
                ignoreObsoleteProperties: _ignoreObsoleteProperties,
                useFullTypeNameInSchemaIds: _useFullTypeNameInSchemaIds,
                describeAllEnumsAsStrings: _describeAllEnumsAsStrings
            );
        }
    }
}