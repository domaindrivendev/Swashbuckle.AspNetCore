using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class SwaggerUIIntegrationTests(ITestOutputHelper outputHelper)
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
        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync(requestPath, TestContext.Current.CancellationToken);

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
        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath, TestContext.Current.CancellationToken);
        AssertResource(htmlResponse);

        using var jsResponse = await client.GetAsync(swaggerUijsPath, TestContext.Current.CancellationToken);
        AssertResource(jsResponse, weakETag: false);

        using var indexCss = await client.GetAsync(indexCssPath, TestContext.Current.CancellationToken);
        AssertResource(indexCss, weakETag: false);

        using var cssResponse = await client.GetAsync(swaggerUiCssPath, TestContext.Current.CancellationToken);
        AssertResource(cssResponse, weakETag: false);

        static void AssertResource(HttpResponseMessage response, bool weakETag = true)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.Equal(weakETag, response.Headers.ETag.IsWeak);
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
        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync(indexJsPath, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
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
        var site = new TestSite(typeof(CustomUIConfig.Startup), outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/swagger/index.js", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"plugins\":[\"customPlugin1\",\"customPlugin2\"]", jsContent);
    }

    [Fact]
    public async Task IndexUrl_DoesntDefinePlugins()
    {
        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/index.js", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        var jsContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("\"plugins\"", jsContent);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomPageTitleAndStylesheets_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("<title>CustomUIConfig</title>", content);
        Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomIndexHtml_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIIndex.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Example.com", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsInterceptors_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.js", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("\"RequestInterceptorFunction\":", content);
        Assert.Contains("\"ResponseInterceptorFunction\":", content);
    }

    [Theory]
    [InlineData("/swagger/index.html", "/swagger/index.js", new[] { "Version 1.0", "Version 2.0" })]
    [InlineData("/swagger/1.0/index.html", "/swagger/1.0/index.js", new[] { "Version 1.0" })]
    [InlineData("/swagger/2.0/index.html", "/swagger/2.0/index.js", new[] { "Version 2.0" })]
    public async Task SwaggerUIMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string jsUrl, string[] versions)
    {
        var site = new TestSite(typeof(MultipleVersions.Startup), outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlUrl, TestContext.Current.CancellationToken);
        using var jsResponse = await client.GetAsync(jsUrl, TestContext.Current.CancellationToken);
        var content = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);

        var content = await htmlResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{cssPath}\">", content);
        Assert.Contains($"<script src=\"{scriptBundlePath}\" charset=\"utf-8\">", content);
        Assert.Contains($"<script src=\"{scriptPresetsPath}\" charset=\"utf-8\">", content);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents()
    {
        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.NotEmpty(embeddedUIFiles);

        foreach (var (resourceName, fileName) in embeddedUIFiles)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fileName);
            using var htmlResponse = await client.SendAsync(requestMessage, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);

            using var stream = await htmlResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            using var diskFileStream = typeof(SwaggerUIIntegrationTests).Assembly.GetManifestResourceStream(resourceName);

            Assert.NotNull(diskFileStream);
            Assert.Equal(SHA1.HashData(diskFileStream), SHA1.HashData(stream));
        }
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents_GZipDirectly()
    {
        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.NotEmpty(embeddedUIFiles);

        foreach (var (resourceName, fileName) in embeddedUIFiles)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fileName);
            requestMessage.Headers.AcceptEncoding.Add(new("gzip"));

            using var htmlResponse = await client.SendAsync(requestMessage, TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.Equal("gzip", htmlResponse.Content.Headers.ContentEncoding.Single());

            using var stream = await htmlResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            using var gzipStream = new GZipStream(stream, CompressionMode.Decompress);
            using var diskFileStream = typeof(SwaggerUIIntegrationTests).Assembly.GetManifestResourceStream(resourceName);

            Assert.NotNull(diskFileStream);
            Assert.Equal(SHA1.HashData(diskFileStream), SHA1.HashData(gzipStream));
        }
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents_NotModified()
    {
        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        var embeddedUIFiles = GetEmbeddedUIFiles();
        Assert.NotEmpty(embeddedUIFiles);

        foreach (var (_, fileName) in embeddedUIFiles)
        {
            using var htmlResponse = await client.GetAsync(fileName, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
            Assert.NotNull(htmlResponse.Headers.ETag?.Tag);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, fileName);
            requestMessage.Headers.IfNoneMatch.Add(new(htmlResponse.Headers.ETag?.Tag));

            using var secondHtmlResponse = await client.SendAsync(requestMessage, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.NotModified, secondHtmlResponse.StatusCode);

            using var stream = await secondHtmlResponse.Content.ReadAsStreamAsync(TestContext.Current.CancellationToken);
            Assert.Equal(0, stream.Length);
        }
    }

    private static List<(string ResourceName, string FileName)> GetEmbeddedUIFiles()
    {
        const string ResourcePrefix = "Swashbuckle.AspNetCore.IntegrationTests.Embedded.SwaggerUI.";
        return typeof(SwaggerUIIntegrationTests).Assembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix))
            .Select(name => (name, name.Substring(ResourcePrefix.Length)))
            .ToList();
    }
}
