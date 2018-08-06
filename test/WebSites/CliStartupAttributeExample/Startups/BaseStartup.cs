using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;

namespace CliStartupAttributeExample.Startups
{
    public abstract class BaseStartup
    {
        public IConfiguration Configuration { get; }

        protected IApplicationFeatureProvider _controllerFilter;

        protected abstract ApiInfo ApiInfo { get; }

        protected string SwaggerDocumentVersion { get; }

        protected BaseStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            SwaggerDocumentVersion = $"v{ApiInfo.ApiVersion.ToString()}";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Separate Public and Private APIs
            services.AddMvc().ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Clear();
                manager.FeatureProviders.Add(_controllerFilter);
            });

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiInfo.ApiVersion;
            });

            services.AddMvcCore().AddApiExplorer();

            string packageVersion = PlatformServices.Default.Application.ApplicationVersion;
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SwaggerDocumentVersion,
                    new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Title = ApiInfo.Title,
                        Description = ApiInfo.Description,
                        Version = packageVersion
                    });
            });

            ConfigureMvcServices(services);

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            ConfigureMvc(app, env, loggerFactory);

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(
                    $"/swagger/{SwaggerDocumentVersion}/swagger.json",
                    $"{SwaggerDocumentVersion} docs");
            });

            app.UseMvc();
        }

        public abstract void ConfigureMvcServices(IServiceCollection services);

        public abstract void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory);
    }
}
