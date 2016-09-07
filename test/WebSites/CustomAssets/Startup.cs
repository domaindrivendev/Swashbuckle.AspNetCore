using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CustomAssets
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi(c =>
            {
                c.InjectJavaScript("/ext/custom-script.js");
                c.InjectStylesheet("/ext/custom-stylesheet.css");
            });
        }
    }
}