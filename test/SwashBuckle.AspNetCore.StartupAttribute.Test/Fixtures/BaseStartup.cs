using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test
{
    public abstract class BaseStartup
    {
        public IConfiguration Configuration { get; }

        protected BaseStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddApiExplorer();

            ConfigureMvcServices(services);

            return services.BuildServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            ConfigureMvc(app, env, loggerFactory);

            app.UseMvc();
        }

        public abstract void ConfigureMvcServices(IServiceCollection services);

        public abstract void ConfigureMvc(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory);
    }
}
