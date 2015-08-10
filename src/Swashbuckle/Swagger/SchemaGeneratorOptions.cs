using System;
using System.Collections.Generic;

namespace Swashbuckle.Swagger
{
    public class SchemaGeneratorOptions
    {
        public SchemaGeneratorOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            ModelFilters = new List<IModelFilter>();
            SchemaIdSelector = (type) => type.FriendlyId(false);
        }

        public IDictionary<Type, Func<Schema>> CustomTypeMappings { get; private set; }

        public IList<IModelFilter> ModelFilters { get; private set; }

        public bool IgnoreObsoleteProperties { get; set; }

        public Func<Type, string> SchemaIdSelector { get; set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }

        public void MapType<T>(Func<Schema> schemaFactory)
        {
            CustomTypeMappings.Add(typeof(T), schemaFactory);
        }

        public void ModelFilter<TFilter>()
            where TFilter : IModelFilter, new()
        {
            ModelFilters.Add(new TFilter());
        }

        public void UseFullTypeNameInSchemaIds()
        {
            SchemaIdSelector = (type) => type.FriendlyId(true);
        }
    }
}