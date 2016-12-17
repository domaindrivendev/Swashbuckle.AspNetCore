using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IdentityServer4.Models;
using IdentityServer4.Configuration;
using Swashbuckle.Swagger.Model;
using OAuth2Integration.ResourceServer.Swagger;

namespace OAuth2Integration
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register IdentityServer services to power OAuth2.0 flows
            services.AddIdentityServer()
                .AddInMemoryClients(AuthServer.Config.Clients())
                .AddInMemoryApiResources(AuthServer.Config.ApiResources())
                .AddInMemoryUsers(AuthServer.Config.Users())
                .AddTemporarySigningCredential();
            services.Configure<IdentityServerOptions>(c =>
            {
                c.AuthenticationOptions.AuthenticationScheme = "Cookies";
            });

            // Configure named auth policies that map directly to OAuth2.0 scopes
            services.AddAuthorization(c =>
            {
                c.AddPolicy("readAccess", p => p.RequireClaim("scope", "readAccess"));
                c.AddPolicy("writeAccess", p => p.RequireClaim("scope", "writeAccess"));
            });

            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "OAuth2Integration Example",
                        Version = "v1"
                    }
                );

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
                c.AddSecurityDefinition("oauth2", new OAuth2Scheme
                {
                    Type = "oauth2",
                    Flow = "implicit",
                    AuthorizationUrl = "/auth-server/connect/authorize",
                    Scopes = new Dictionary<string, string>
                    {
                        { "readAccess", "Access read operations" },
                        { "writeAccess", "Access write operations" }
                    }
                });
                // Assign scope requirements to operations based on AuthorizeAttribute
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseDeveloperExceptionPage();

            app.Map("/auth-server", authServer =>
            {
                authServer.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    AuthenticationScheme = "Cookies"
                });

                authServer.UseIdentityServer();
                authServer.UseMvc();
            });

            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = "http://localhost:50581/auth-server/",
                    RequireHttpsMetadata = false
                });

                resourceServer.UseMvc();

                resourceServer.UseSwagger(documentFilter: (swaggerDoc, req) =>
                {
                    swaggerDoc.Host = req.Host.Value;
                });

                resourceServer.UseSwaggerUi(c =>
                {
                    c.SwaggerEndpoint("/resource-server/swagger/v1/swagger.json", "My API V1");
                    c.ConfigureOAuth2("swagger-ui", "swagger-ui-secret".Sha256(), "swagger-ui-realm", "Swagger UI");
                });
            });
        }
    }
}
