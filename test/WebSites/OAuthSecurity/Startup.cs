using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.TestSupport;
using OAuthSecurity.Swagger;

namespace OAuthSecurity
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
            // Register IdentityServer services to power OAuth2.0 flows
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(AuthServer.Config.ApiScopes())
                .AddInMemoryApiResources(AuthServer.Config.ApiResources())
                .AddInMemoryClients(AuthServer.Config.Clients())
                .AddTestUsers(AuthServer.Config.Users());

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddJwtBearer(c =>
                {
                    c.Authority = "http://localhost:50134/auth-server";
                    c.RequireHttpsMetadata = false;
                    c.Audience = "api";
                });

            services.AddAuthorization(c =>
            {
                c.AddPolicy("readAccess", p =>
                {
                    p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireClaim("scope", "readOperations");
                });
                c.AddPolicy("writeAccess", p =>
                {
                    p.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    p.RequireClaim("scope", "writeOperations");
                });
            });

            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri("/auth-server/connect/authorize", UriKind.Relative),
                            TokenUrl = new Uri("/auth-server/connect/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "readOperations", "Access read operations" },
                                { "writeOperations", "Access write operations" }
                            }
                        }
                    }
                });

                c.OperationFilter<OAuthOperationFilter>();
            });
            services.ConfigureSwaggerGen(c => SwaggerSectionMarkers.Enable(c));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                // Default values to display in the auth popup
                c.OAuthClientId("test-client");
                c.OAuthClientSecret("test-secret");
                c.OAuthAppName("test-app");
                c.OAuthScopes("readOperations");

                // Seperator for passing required scopes in query string (default is a space)
                c.OAuthScopeSeparator(" ");

                // Use PCKE for enhanced security (only applicable for authorization code flow)
                c.OAuthUsePkce();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Map("/auth-server", authServer =>
            {
                authServer.UseRouting();

                authServer.UseIdentityServer();

                authServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
