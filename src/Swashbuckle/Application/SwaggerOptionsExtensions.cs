using System;
using Swashbuckle.Swagger;

namespace Swashbuckle.Swagger
{
    public static class SwaggerOptionsExtensions
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

        public static void OperationFilter<TFilter>(this SwaggerGeneratorOptions options)
            where TFilter: IOperationFilter, new()
        {
            options.OperationFilters.Add(new TFilter());
        }

        public static void DocumentFilter<TFilter>(this SwaggerGeneratorOptions options)
            where TFilter: IDocumentFilter, new()
        {
            options.DocumentFilters.Add(new TFilter());
        }
    }
}
