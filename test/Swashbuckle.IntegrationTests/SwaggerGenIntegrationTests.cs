using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Swashbuckle.IntegrationTests
{
    public class SwaggerGenIntegrationTests
    {
        private Type _startupType;
        private TestServer _server;
        private readonly ITestOutputHelper _output;
        private readonly HttpClient _validatorClient;

        public SwaggerGenIntegrationTests(ITestOutputHelper output)
        {
            _output = output;
            _validatorClient = new HttpClient
            {
                BaseAddress = new Uri("http://online.swagger.io")
            };
        }

        [Theory] //[Theory(Skip = "online.swagger.io/validator/debug is currently unavailable")]
        // The filter that adds the default in the Basic site doesn't validate right:
        // {"messages":["attribute definitions.Cart.default is not of type `string`"]}
        // TODO: [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
		[InlineData(typeof(CustomizedUi.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/v2/swagger.json")]
        [InlineData(typeof(SecuritySchemes.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(VirtualDirectory.Startup), "/vdir/swagger/v1/swagger.json")]
        public async Task SwaggerRequest_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            _startupType = startupType;

            var siteContentRoot = GetApplicationPath(@"..\..\..\..\WebSites\");

            var builder = new WebHostBuilder()
                  .UseContentRoot(siteContentRoot)
                  .ConfigureServices(InitializeServices)
                  .UseStartup(startupType);

            _server = new TestServer(builder);

            var client = _server.CreateClient();
            client.BaseAddress = new Uri("http://localhost");

            var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            swaggerResponse.EnsureSuccessStatusCode();
            await AssertValidSwaggerAsync(swaggerResponse);
        }

        private async Task AssertValidSwaggerAsync(HttpResponseMessage swaggerResponse)
        {
            var validationResponse = await _validatorClient.PostAsync("/validator/debug", swaggerResponse.Content);

            validationResponse.EnsureSuccessStatusCode();
            var validationErrorsString = await validationResponse.Content.ReadAsStringAsync();
            _output.WriteLine(validationErrorsString);
            Assert.Equal("{}", validationErrorsString);
        }

        private string GetApplicationPath(string relativePath)
        {
            var startupAssembly = _startupType.GetTypeInfo().Assembly;
            var applicationName = startupAssembly.GetName().Name;
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            return Path.GetFullPath(Path.Combine(applicationBasePath, relativePath, applicationName));
        }

        protected virtual void InitializeServices(IServiceCollection services)
        {
            var startupAssembly = _startupType.GetTypeInfo().Assembly;

            // Inject a custom application part manager. Overrides AddMvcCore() because that uses TryAdd().
            var manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(startupAssembly));

            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            manager.FeatureProviders.Add(new ViewComponentFeatureProvider());

            services.Configure<MvcOptions>(c => c.Conventions.Add(new TestAppActionModel(_startupType.GetTypeInfo().Assembly)));

            services.AddSingleton(manager);
        }
    }
}