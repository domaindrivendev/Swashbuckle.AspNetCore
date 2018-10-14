using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.AzureFunctions.Annotations;
using Swashbuckle.AspNetCore.AzureFunctions.Extensions;
using Swashbuckle.AspNetCore.AzureFunctions.Filters;

namespace FunctionApp
{
    public static class GenerateSwaggerFunction
    {
        //static GenerateSwaggerFunction()
        //{
        //    RedirectAssembly();
        //}

        //public static void RedirectAssembly()
        //{
        //    var list = AppDomain.CurrentDomain.GetAssemblies()
        //        .Select(a => a.GetName())
        //        .OrderByDescending(a => a.Name)
        //        .ThenByDescending(a => a.Version)
        //        .Select(a => a.FullName)
        //        .ToList();

        //    //var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //    //var allAssemblies = 

        //    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        //    {
        //        var requestedAssembly = new AssemblyName(args.Name);
        //        foreach (string asmName in list)
        //        {
        //            if (asmName.StartsWith(requestedAssembly.Name + ","))
        //            {
        //                return Assembly.Load(asmName);
        //            }
        //        }
        //        return null;
        //    };
        //}

        [FunctionName("GenerateSwaggerFunction")]
        [SwaggerIgnore]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "swagger")]
            HttpRequestMessage req, TraceWriter log)
        {
            var services = new ServiceCollection();

            var functionAssembly = Assembly.GetExecutingAssembly();
            services.AddAzureFunctionsApiProvider(functionAssembly);

            // Add Swagger Configuration
            services.AddSwaggerGen(options =>
            {
                // SwaggerDoc - API
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Sample",
                    Version = "v1",
                    Description = "Sample function",
                    Contact = new Contact
                    {
                        Name = "Jon Doe",
                        Email = "jon.doe@someemailprovider.foo"
                    }
                });

                options.TryIncludeFunctionXmlComments(functionAssembly);

                // Add Enums to Swagger as String
                options.DescribeAllEnumsAsStrings();

            });

            var serviceProvider = services.BuildServiceProvider(true);
            var content = serviceProvider.GetSwagger("v1");

            return new HttpResponseMessage()
            {
                Content = new StringContent(content)
            };
        }
    }
}
