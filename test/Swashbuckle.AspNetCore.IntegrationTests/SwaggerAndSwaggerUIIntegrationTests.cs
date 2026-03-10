using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Swashbuckle.AspNetCore.IntegrationTests;

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

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    // MapSwagger()
    [InlineData("/swagger/v1/swagger.json", "application/json")]
    [InlineData("/swagger/v1/swagger.yaml", "text/yaml")]
    [InlineData("/swagger/v1/swagger.yml", "text/yaml")]
    // MapSwaggerUI()
    [InlineData("/swagger/index.html", "text/html")]
    // MapReDoc()
    [InlineData("/api-docs/index.html", "text/html")]
    [InlineData("/api-docs/index.js", "application/javascript")]
    public async Task Map_Methods_ReturnExpectedEndpoints(string path, string mediaType)
    {
        var client = new WebApplicationFactory<WebApi.Map.Program>().CreateClient();

        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(mediaType, response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    // MapSwaggerUI().RequireAuthorization()
    [InlineData("/swagger-auth/index.html")]
    // MapReDoc().RequireAuthorization()
    [InlineData("/redoc-auth/index.html")]
    public async Task MapSwaggerUI_And_MapReDoc_RequireAuthorization_ReturnUnauthorized(string path)
    {
        var client = new WebApplicationFactory<WebApi.Map.Program>().CreateClient();
        var response = await client.GetAsync(path, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
