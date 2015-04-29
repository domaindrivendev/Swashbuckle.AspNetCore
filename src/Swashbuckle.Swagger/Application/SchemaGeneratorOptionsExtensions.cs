using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public static class SchemaGeneratorOptionsExtensions
    {
        public static void MapType<T>(this SchemaGeneratorOptions options, Func<Schema> schemaFactory)
        {
            options.CustomTypeMappings.Add(typeof(T), schemaFactory);
        }

        public static void ModelFilter<TFilter>(this SchemaGeneratorOptions options)
            where TFilter : IModelFilter, new()
        {
            options.ModelFilters.Add(new TFilter());
        }
    }
}