using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace CustomUIConfig
{
    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Version = "v1", Title = "Swashbuckle Sample API" });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

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
            });

            app.UseStaticFiles();
            app.UseMvc();
        }
    }
}