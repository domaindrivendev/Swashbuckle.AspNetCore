using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.Newtonsoft;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenNewtonsoftSupport(this IServiceCollection services)
        {
            return services.AddTransient<IApiModelResolver, NewtonsoftApiModelResolver>();
        }
    }
}
