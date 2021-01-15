using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace OAuth2Integration
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
                .AddInMemoryClients(AuthServer.Config.Clients())
                .AddInMemoryApiResources(AuthServer.Config.ApiResources())
                .AddTestUsers(AuthServer.Config.TestUsers());

            // The auth setup is a little nuanced because this app provides the auth-server & the resource-server
            // Use the "Cookies" scheme by default & explicitly require "Bearer" in the resource-server controllers
            // See https://docs.microsoft.com/en-us/aspnet/core/security/authorization/limitingidentitybyscheme?tabs=aspnetcore2x
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddIdentityServerAuthentication(c =>
                {
                    c.Authority = "http://localhost:55202/auth-server/";
                    c.RequireHttpsMetadata = false;
                    c.ApiName = "api";
                });

            // Configure named auth policies that map directly to OAuth2.0 scopes
            services.AddAuthorization(c =>
            {
                c.AddPolicy("readAccess", p => p.RequireClaim("scope", "readAccess"));
                c.AddPolicy("writeAccess", p => p.RequireClaim("scope", "writeAccess"));
            });

            services.AddControllersWithViews();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Test API V1" });

                // Define the OAuth2.0 scheme that's in use (i.e. Implicit Flow)
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
                                { "readAccess", "Access read operations" },
                                { "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new[] { "readAccess", "writeAccess" }
                    }
                });

                // Assign scope requirements to operations based on AuthorizeAttribute
                //c.OperationFilter<SecurityRequirementsOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/auth-server", authServer =>
            {
                authServer.UseRouting();

                authServer.UseAuthentication();

                authServer.UseIdentityServer();

                authServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            });

            app.Map("/resource-server", resourceServer =>
            {
                resourceServer.UseRouting();

                resourceServer.UseAuthentication();

                resourceServer.UseAuthorization();

                resourceServer.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

                resourceServer.UseSwagger();
                resourceServer.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/resource-server/swagger/v1/swagger.json", "My API V1");
                    c.EnableDeepLinking();

                    // Additional OAuth settings (See https://github.com/swagger-api/swagger-ui/blob/v3.10.0/docs/usage/oauth2.md)
                    c.OAuthClientId("test-id");
                    c.OAuthClientSecret("test-secret");
                    c.OAuthAppName("test-app");
                    c.OAuthScopeSeparator(" ");
                    c.OAuthScopes("readAccess");
                    c.OAuthUsePkce();
                });
            });
        }
    }
}
