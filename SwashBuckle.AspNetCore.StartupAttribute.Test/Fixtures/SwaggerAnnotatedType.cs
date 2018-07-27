using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures
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

    public class StartupPublic : BaseStartup
    {
        private static readonly ApiVersion ApiVersion = new ApiVersion(1, 0);

        protected override ApiInfo ApiInfo => new ApiInfo("Public test API", "Public test API description", ApiVersion);

        public StartupPublic(IConfiguration configuration) : base(configuration)
        {
            _controllerFilter = new TypedControllerFeatureProvider<Controller>();
        }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureMvcServices(IServiceCollection services)
        {
            throw new NotImplementedException();
        }
    }

    public class StartupPrivate : BaseStartup
    {
        private static readonly ApiVersion ApiVersion = new ApiVersion(1, 0);

        protected override ApiInfo ApiInfo => new ApiInfo("Private test API", "Private test API description", ApiVersion);

        public StartupPrivate(IConfiguration configuration) : base(configuration)
        {
            _controllerFilter = new TypedControllerFeatureProvider<PrivateApiController>();
        }

        public override void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            throw new NotImplementedException();
        }

        public override void ConfigureMvcServices(IServiceCollection services)
        {
            throw new NotImplementedException();
        }
    }
}
