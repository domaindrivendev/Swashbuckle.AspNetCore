using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Swashbuckle.AspNetCore.IntegrationTests
{
    public class SwaggerAndSwaggerUIIntegrationTests
    {
        [Theory]
        [InlineData("/swagger/index.html", "text/html")]
        [InlineData("/swagger/v1.json", "application/json")]
        [InlineData("/swagger/v1.yaml", "text/yaml")]
        [InlineData("/swagger/v1.yml", "text/yaml")]
        public async Task SwaggerDocWithoutSubdirectory(string path, string mediaType)
        {
            var client = new WebApplicationFactory<TopLevelSwaggerDoc.Program>().CreateClient();

            var response = await client.GetAsync(path);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
        }
    }
}