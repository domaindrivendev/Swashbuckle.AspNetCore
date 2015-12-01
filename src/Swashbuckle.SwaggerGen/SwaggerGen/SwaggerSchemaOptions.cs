using System;
using System.Collections.Generic;

namespace Swashbuckle.SwaggerGen
{
    public class SwaggerSchemaOptions
    {
        public SwaggerSchemaOptions()
        {
            CustomTypeMappings = new Dictionary<Type, Func<Schema>>();
            SchemaIdSelector = (type) => type.FriendlyId(false);
            ModelFilters = new List<IModelFilter>();
        }

        internal IDictionary<Type, Func<Schema>> CustomTypeMappings { get; private set; }

        public bool DescribeAllEnumsAsStrings { get; set; }

        public bool DescribeStringEnumsInCamelCase { get; set; }

        internal Func<Type, string> SchemaIdSelector { get; set; }

        public bool IgnoreObsoleteProperties { get; set; }

        internal IList<IModelFilter> ModelFilters { get; private set; }

        public void MapType<T>(Func<Schema> schemaFactory)
        {
            CustomTypeMappings.Add(typeof(T), schemaFactory);
        }

        public void CustomSchemaIds(Func<Type, string> schemaIdSelector)
        {
            SchemaIdSelector = schemaIdSelector;
        }

        public void ModelFilter<TFilter>()
            where TFilter : IModelFilter, new()
        {
            ModelFilter(new TFilter());
        }

        public void ModelFilter<TFilter>(TFilter filter)
            where TFilter : IModelFilter
        {
            ModelFilters.Add(filter);
        }
    }
}