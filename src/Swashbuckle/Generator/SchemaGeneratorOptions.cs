using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger.Generator
{
    public class SchemaGeneratorOptions
    {
        public SchemaGeneratorOptions(
            IReadOnlyDictionary<Type, Func<Schema>> customTypeMappings = null,
            IEnumerable<ISchemaFilter> schemaFilters = null,
            bool ignoreObsoleteProperties = false,
            bool useFullTypeNameInSchemaIds = false,
            bool describeAllEnumsAsStrings = false,
            string schemaReferencePrefix = "#/definitions/")
        {
            CustomTypeMappings = customTypeMappings ?? new Dictionary<Type, Func<Schema>>();
            SchemaFilters = schemaFilters ?? new ISchemaFilter[] { };
            IgnoreObsoleteProperties = ignoreObsoleteProperties;
            UseFullTypeNameInSchemaIds = useFullTypeNameInSchemaIds;
            DescribeAllEnumsAsStrings = describeAllEnumsAsStrings;
            SchemaReferencePrefix = schemaReferencePrefix;
        }

        public IReadOnlyDictionary<Type, Func<Schema>> CustomTypeMappings { get; private set; }

        public IEnumerable<ISchemaFilter> SchemaFilters { get; private set; }

        public bool IgnoreObsoleteProperties { get; private set; }

        public bool UseFullTypeNameInSchemaIds { get; private set; }

        public bool DescribeAllEnumsAsStrings { get; private set; }

        public string SchemaReferencePrefix { get; private set; }
    }
}