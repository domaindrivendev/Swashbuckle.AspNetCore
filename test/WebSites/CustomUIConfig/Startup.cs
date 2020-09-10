using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CustomUIConfig
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                // Core
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");

                // Display
                c.DefaultModelExpandDepth(2);
                c.DefaultModelRendering(ModelRendering.Model);
                c.DefaultModelsExpandDepth(-1);
                c.DisplayOperationId();
                c.DisplayRequestDuration();
                c.DocExpansion(DocExpansion.None);
                c.EnableDeepLinking();
                c.EnableFilter();
                c.ShowExtensions();

                // Network
                c.EnableValidator();
                c.SupportedSubmitMethods(SubmitMethod.Get);

                // Other
                c.DocumentTitle = "CustomUIConfig";
                c.InjectStylesheet("/ext/custom-stylesheet.css");
                c.InjectJavascript("/ext/custom-javascript.js");
                c.UseRequestInterceptor("(req) => { req.headers['x-my-custom-header'] = 'MyCustomValue'; return req; }");
                c.UseResponseInterceptor("(res) => { console.log('Custom interceptor intercepted response from:', res.url); return res; }");
            });
        }
    }
}
