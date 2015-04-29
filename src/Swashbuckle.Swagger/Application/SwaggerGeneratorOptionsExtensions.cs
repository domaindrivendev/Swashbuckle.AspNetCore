using Swashbuckle.Swagger;

namespace Swashbuckle.Application
{
    public static class SwaggerGeneratorOptionsExtensions
    {
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
