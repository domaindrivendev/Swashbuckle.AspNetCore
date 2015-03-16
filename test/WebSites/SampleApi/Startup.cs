using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.DependencyInjection;
using Swashbuckle.Application;

namespace SampleApi
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSwashbuckle();

            // TODO: Encapsulate below as default behavior behind AddSwashbuckle
            services.Configure<MvcOptions>(options =>
            {
                options.ApplicationModelConventions.Add(new ApiExplorerForSwaggerConvention());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
