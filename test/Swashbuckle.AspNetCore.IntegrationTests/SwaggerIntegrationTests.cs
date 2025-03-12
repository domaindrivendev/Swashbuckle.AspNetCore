using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Readers;
using Xunit;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    [Collection("TestSite")]
    public class SwaggerIntegrationTests
    {
        [Theory]
        [InlineData(typeof(Basic.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CliExample.Startup), "/swagger/v1/swagger_net8.0.json")]
        [InlineData(typeof(ConfigFromFile.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIConfig.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(CustomUIIndex.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(GenericControllers.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/1.0/swagger.json")]
        [InlineData(typeof(MultipleVersions.Startup), "/swagger/2.0/swagger.json")]
        [InlineData(typeof(NSwagClientExample.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(OAuth2Integration.Startup), "/resource-server/swagger/v1/swagger.json")]
        [InlineData(typeof(ReDocApp.Startup), "/swagger/v1/swagger.json")]
        [InlineData(typeof(TestFirst.Startup), "/swagger/v1-generated/openapi.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson(
            Type startupType,
            string swaggerRequestUri)
        {
            var testSite = new TestSite(startupType);
            using var client = testSite.BuildClient();

            await AssertValidSwaggerJson(client, swaggerRequestUri);
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_ForAutofaq()
        {
            var testSite = new TestSiteAutofaq(typeof(CliExampleWithFactory.Startup));
            using var client = testSite.BuildClient();

            await AssertValidSwaggerJson(client, "/swagger/v1/swagger_net8.0.json");
        }

        [Fact]
        public async Task SwaggerEndpoint_ReturnsNotFound_IfUnknownSwaggerDocument()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync("/swagger/v2/swagger.json");

            Assert.Equal(System.Net.HttpStatusCode.NotFound, swaggerResponse.StatusCode);
        }

        [Fact]
        public async Task SwaggerEndpoint_DoesNotReturnByteOrderMark()
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync("/swagger/v1/swagger.json");

            swaggerResponse.EnsureSuccessStatusCode();
            var contentBytes = await swaggerResponse.Content.ReadAsByteArrayAsync();
            var bomBytes = Encoding.UTF8.GetPreamble();
            Assert.NotEqual(bomBytes, contentBytes.Take(bomBytes.Length));
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("sv-SE")]
        public async Task SwaggerEndpoint_ReturnsCorrectPriceExample_ForDifferentCultures(string culture)
        {
            var testSite = new TestSite(typeof(Basic.Startup));
            using var client = testSite.BuildClient();

            using var swaggerResponse = await client.GetAsync($"/swagger/v1/swagger.json?culture={culture}");

            swaggerResponse.EnsureSuccessStatusCode();
            using var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            var currentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            try
            {
                var openApiDocument = new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);
                var example = openApiDocument.Components.Schemas["Product"].Example as OpenApiObject;
                var price = (example["price"] as OpenApiDouble);
                Assert.NotNull(price);
                Assert.Equal(14.37, price.Value);
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
            }
        }

        [Theory]
        [InlineData("/swagger/v1/swagger.json", "openapi", "3.0.1")]
        [InlineData("/swagger/v1/swaggerv2.json", "swagger", "2.0")]
        public async Task SwaggerMiddleware_CanBeConfiguredMultipleTimes(
            string swaggerUrl,
            string expectedVersionProperty,
            string expectedVersionValue)
        {
            using var client = new TestSite(typeof(Basic.Startup)).BuildClient();

            using var response = await client.GetAsync(swaggerUrl);

            response.EnsureSuccessStatusCode();
            using var contentStream = await response.Content.ReadAsStreamAsync();

            var json = await JsonSerializer.DeserializeAsync<JsonElement>(contentStream);
            Assert.Equal(expectedVersionValue, json.GetProperty(expectedVersionProperty).GetString());
        }

        [Theory]
        [InlineData(typeof(MinimalApp.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(TopLevelSwaggerDoc.Program), "/swagger/v1.json")]
        [InlineData(typeof(MvcWithNullable.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(WebApi.Program), "/swagger/v1/swagger.json")]
        [InlineData(typeof(WebApi.Aot.Program), "/swagger/v1/swagger.json")]
        public async Task SwaggerEndpoint_ReturnsValidSwaggerJson_Without_Startup(
            Type entryPointType,
            string swaggerRequestUri)
        {
            await SwaggerEndpointReturnsValidSwaggerJson(entryPointType, swaggerRequestUri);
        }

        [Fact]
        public async Task TypesAreRenderedCorrectly()
        {
            using var application = new TestApplication<WebApi.Program>();
            using var client = application.CreateDefaultClient();

            using var swaggerResponse = await client.GetFromJsonAsync<JsonDocument>("/swagger/v1/swagger.json");

            var weatherForecase = swaggerResponse.RootElement
                .GetProperty("components")
                .GetProperty("schemas")
                .GetProperty("WeatherForecast");

            Assert.Equal("object", weatherForecase.GetProperty("type").GetString());

            var properties = weatherForecase.GetProperty("properties");
            Assert.Equal(4, properties.EnumerateObject().Count());

            Assert.Multiple(
            [
                () => Assert.Equal("string", properties.GetProperty("date").GetProperty("type").GetString()),
                () => Assert.Equal("date", properties.GetProperty("date").GetProperty("format").GetString()),
                () => Assert.Equal("integer", properties.GetProperty("temperatureC").GetProperty("type").GetString()),
                () => Assert.Equal("int32", properties.GetProperty("temperatureC").GetProperty("format").GetString()),
                () => Assert.Equal("string", properties.GetProperty("summary").GetProperty("type").GetString()),
                () => Assert.True(properties.GetProperty("summary").GetProperty("nullable").GetBoolean()),
                () => Assert.Equal("integer", properties.GetProperty("temperatureF").GetProperty("type").GetString()),
                () => Assert.Equal("int32", properties.GetProperty("temperatureF").GetProperty("format").GetString()),
                () => Assert.True(properties.GetProperty("temperatureF").GetProperty("readOnly").GetBoolean()),
            ]);
        }

        private static async Task SwaggerEndpointReturnsValidSwaggerJson(Type entryPointType, string swaggerRequestUri)
        {
            using var client = GetHttpClientForTestApplication(entryPointType);
            await AssertValidSwaggerJson(client, swaggerRequestUri);
        }

        internal static HttpClient GetHttpClientForTestApplication(Type entryPointType)
        {
            var applicationType = typeof(TestApplication<>).MakeGenericType(entryPointType);
            var application = (IDisposable)Activator.CreateInstance(applicationType);
            Assert.NotNull(application);

            var createClientMethod = applicationType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(m => m.Name == "CreateDefaultClient" && m.GetParameters().Length == 1);
            if (createClientMethod == null)
            {
                throw new InvalidOperationException($"The method CreateDefaultClient was not found on TestApplication<{entryPointType.FullName}>.");
            }

            // Pass null for DelegatingHandler[]
            var parameters = new object[] { null };

            var clientObject = (IDisposable)createClientMethod.Invoke(application, parameters);
            if (clientObject is not HttpClient client)
            {
                throw new InvalidOperationException($"The method CreateDefaultClient on TestApplication<{entryPointType.FullName}> did not return an HttpClient.");
            }

            return client;
        }

        private static async Task AssertValidSwaggerJson(HttpClient client, string swaggerRequestUri)
        {
            using var swaggerResponse = await client.GetAsync(swaggerRequestUri);

            Assert.True(swaggerResponse.IsSuccessStatusCode, $"IsSuccessStatusCode is false. Response: '{await swaggerResponse.Content.ReadAsStringAsync()}'");
            using var contentStream = await swaggerResponse.Content.ReadAsStreamAsync();
            new OpenApiStreamReader().Read(contentStream, out OpenApiDiagnostic diagnostic);
            Assert.Empty(diagnostic.Errors);
        }
    }
}
