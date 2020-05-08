using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace CustomUIIndex
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();

            app.Use(async (context, next) =>
            {
                if(!context.Request.Path.StartsWithSegments("/swagger/index.html", StringComparison.OrdinalIgnoreCase))
                {
                    await next();
                    return;
                }

                var scriptNonce = $"script+{Guid.NewGuid():N}";
                var styleNonce = $"style+{Guid.NewGuid():N}";
                context.Items[CspConfigObject.DefaultScriptNonceKey] = scriptNonce;
                context.Items[CspConfigObject.DefaultStyleNonceKey] = styleNonce;
                context.Response.Headers["Content-Security-Policy"] = $"img-src 'self' data:; script-src 'self' 'nonce-{scriptNonce}'; style-src 'self' 'nonce-{styleNonce}';";
                await next();
            });

            app.UseSwaggerUI(c =>
            {
                c.IndexStream = () => GetType().Assembly.GetManifestResourceStream("CustomUIIndex.Swagger.index.html");

                c.CspDisableHeader();
            });
        }
    }
}
