using System.Net;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class SwaggerUIIntegrationTests
{
    [Theory]
    [InlineData(typeof(Basic.Startup), "/", "index.html")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger", "swagger/index.html")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/", "index.html")]
    public async Task RoutePrefix_RedirectsToPathRelativeIndexUrl(
        Type startupType,
        string requestPath,
        string expectedRedirectPath)
    {
        var site = new TestSite(startupType);
        using var client = site.BuildClient();

        using var response = await client.GetAsync(requestPath);

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(expectedRedirectPath, response.Headers.Location.ToString());
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "/index.html", "/swagger-ui.js", "/index.css", "/swagger-ui.css")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html", "/swagger/swagger-ui.js", "swagger/index.css", "/swagger/swagger-ui.css")]
    public async Task IndexUrl_ReturnsEmbeddedVersionOfTheSwaggerUI(
        Type startupType,
        string htmlPath,
        string swaggerUijsPath,
        string indexCssPath,
        string swaggerUiCssPath)
    {
        var site = new TestSite(startupType);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath);
        AssertResource(htmlResponse);

        using var jsResponse = await client.GetAsync(swaggerUijsPath);
        AssertResource(jsResponse);

        using var indexCss = await client.GetAsync(indexCssPath);
        AssertResource(indexCss);

        using var cssResponse = await client.GetAsync(swaggerUiCssPath);
        AssertResource(cssResponse);

        static void AssertResource(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.True(response.Headers.ETag.IsWeak);
            Assert.NotEmpty(response.Headers.ETag.Tag);
            Assert.NotNull(response.Headers.CacheControl);
            Assert.True(response.Headers.CacheControl.Private);
            Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);
        }
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "/index.js")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.js")]
    public async Task SwaggerUIMiddleware_ReturnsInitializerScript(
        Type startupType,
        string indexJsPath)
    {
        var site = new TestSite(startupType);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync(indexJsPath);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync();
        Assert.Contains("SwaggerUIBundle", jsContent);
        Assert.DoesNotContain("%(DocumentTitle)", jsContent);
        Assert.DoesNotContain("%(HeadContent)", jsContent);
        Assert.DoesNotContain("%(StylesPath)", jsContent);
        Assert.DoesNotContain("%(ScriptBundlePath)", jsContent);
        Assert.DoesNotContain("%(ScriptPresetsPath)", jsContent);
        Assert.DoesNotContain("%(ConfigObject)", jsContent);
        Assert.DoesNotContain("%(OAuthConfigObject)", jsContent);
        Assert.DoesNotContain("%(Interceptors)", jsContent);
    }

    [Fact]
    public async Task IndexUrl_DefinesPlugins()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup));
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/swagger/index.js");
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync();
        Assert.Contains("\"plugins\":[\"customPlugin1\",\"customPlugin2\"]", jsContent);
    }

    [Fact]
    public async Task IndexUrl_DoesntDefinePlugins()
    {
        var site = new TestSite(typeof(Basic.Startup));
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/index.js");
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        var jsContent = await jsResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("\"plugins\"", jsContent);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomPageTitleAndStylesheets_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("<title>CustomUIConfig</title>", content);
        Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomIndexHtml_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIIndex.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("Example.com", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsInterceptors_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.js");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"RequestInterceptorFunction\":", content);
        Assert.Contains("\"ResponseInterceptorFunction\":", content);
    }

    [Theory]
    [InlineData("/swagger/index.html", "/swagger/index.js", new[] { "Version 1.0", "Version 2.0" })]
    [InlineData("/swagger/1.0/index.html", "/swagger/1.0/index.js", new[] { "Version 1.0" })]
    [InlineData("/swagger/2.0/index.html", "/swagger/2.0/index.js", new[] { "Version 2.0" })]
    public async Task SwaggerUIMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string jsUrl, string[] versions)
    {
        var site = new TestSite(typeof(MultipleVersions.Startup));
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlUrl);
        using var jsResponse = await client.GetAsync(jsUrl);
        var content = await jsResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        foreach (var version in versions)
        {
            Assert.Contains(version, content);
        }
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "/index.html", "./swagger-ui.css", "./swagger-ui-bundle.js", "./swagger-ui-standalone-preset.js")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html", "/ext/custom-stylesheet.css", "/ext/custom-javascript.js", "/ext/custom-javascript.js")]
    public async Task IndexUrl_Returns_ExpectedAssetPaths(
        Type startupType,
        string htmlPath,
        string cssPath,
        string scriptBundlePath,
        string scriptPresetsPath)
    {
        var site = new TestSite(startupType);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath);
        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);

        var content = await htmlResponse.Content.ReadAsStringAsync();
        Assert.Contains($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{cssPath}\">", content);
        Assert.Contains($"<script src=\"{scriptBundlePath}\" charset=\"utf-8\">", content);
        Assert.Contains($"<script src=\"{scriptPresetsPath}\" charset=\"utf-8\">", content);
    }
}
