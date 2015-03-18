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
        private IList<Func<ISchemaFilter>> _schemaFilters;
        private bool _ignoreObsoleteProperties;
        private bool _useFullTypeNameInSchemaIds;
        private bool _describeAllEnumsAsStrings;
        private string _schemaReferencePrefix;

        public SchemaGeneratorOptionsBuilder()
        {
            _customTypeMappings = new Dictionary<Type, Func<Schema>>();
            _schemaFilters = new List<Func<ISchemaFilter>>();
        }

        public void MapType<T>(Func<Schema> createSchema)
        {
            _customTypeMappings[typeof(T)] = createSchema;
        }

        public void SchemaFilter<TFilter>()
            where TFilter : ISchemaFilter, new()
        {
            SchemaFilter(() => new TFilter());
        }

        public void SchemaFilter(Func<ISchemaFilter> createFilter)
        {
            _schemaFilters.Add(createFilter);
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
            var schemaFilters = _schemaFilters.Select(factory => factory());

            return new SchemaGeneratorOptions(
                customTypeMappings: new ReadOnlyDictionary<Type, Func<Schema>>(_customTypeMappings),
                schemaFilters: schemaFilters,
                ignoreObsoleteProperties: _ignoreObsoleteProperties,
                useFullTypeNameInSchemaIds: _useFullTypeNameInSchemaIds,
                describeAllEnumsAsStrings: _describeAllEnumsAsStrings
            );
        }
    }
}