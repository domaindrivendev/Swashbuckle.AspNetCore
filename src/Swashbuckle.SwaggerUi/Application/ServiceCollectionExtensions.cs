using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Mvc;

namespace Swashbuckle.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSwaggerUi(
            this IServiceCollection serviceCollection,
            string customRoute = null)
        {
            serviceCollection.Configure<MvcOptions>(c =>
                c.ApplicationModelConventions.Add(new SwaggerUiApplicationConvention(customRoute)));
        }
    }
}