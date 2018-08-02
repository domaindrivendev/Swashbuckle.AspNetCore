using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

using SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Startups
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
            // Api versioning
            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiInfo.ApiVersion;
            });

            services.AddMvcCore().AddApiExplorer();

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

            app.UseMvc();
        }

        public abstract void ConfigureMvcServices(IServiceCollection services);

        public abstract void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory);
    }
}
