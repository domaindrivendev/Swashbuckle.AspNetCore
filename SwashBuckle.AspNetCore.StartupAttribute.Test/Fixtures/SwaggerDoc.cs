using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwashBuckle.AspNetCore.StartupAttribute.Test.Fixtures
{
    public static class SwaggerDoc
    {
        public static void ConfigureSwaggerMvcServices(this IServiceCollection services, string SwaggerDocumentVersion, ApiInfo apiInfo, string assemblyName)
        {
            //string packageVersion = PlatformServices.Default.Application.ApplicationVersion;

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc(SwaggerDocumentVersion,
            //        new Info
            //        {
            //            Version = packageVersion,
            //            Title = apiInfo.Title,
            //            Description = apiInfo.Description,
            //            TermsOfService = "GMV all rights reserved",
            //            Contact = new Contact { Name = "ITS SW Team" },
            //            License = new License { Name = "GMV-ITS License", Url = "http://www.gmv.com" }
            //        }
            //    );

            //    // Set the comments path for the Swagger JSON and UI.
            //    c.IncludeXmlComments(StartupHelpers.GetXmlCommentsFilePath(assemblyName));
            //});
        }

        public static void ConfigureSwaggerMvc(this IApplicationBuilder app, string SwaggerDocumentVersion)
        {
            //app.UseSwagger(c =>
            //{
            //    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
            //});

            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint($"/swagger/{SwaggerDocumentVersion}/swagger.json", $"{SwaggerDocumentVersion} docs");
            //});
        }
    }
}
