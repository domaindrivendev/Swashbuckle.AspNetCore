using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class SwaggerUIIntegrationTests(ITestOutputHelper outputHelper)
{
    private const string EmptyStringSha256Hash = "2jmj7l5rSw0yVb/vlWAYkK/YBwk=";

    public static TheoryData<string, string> SwaggerUIFiles()
    {
        const string Prefix = "Swashbuckle.AspNetCore.IntegrationTests.Embedded.SwaggerUI.";

        var resources = typeof(SwaggerUIIntegrationTests).Assembly
            .GetManifestResourceNames()
            .Where((p) => p.StartsWith(Prefix))
            .Select((p) => (p, p[Prefix.Length..]))
            .ToList();

        var testCases = new TheoryData<string, string>();

        foreach (var (resourceName, fileName) in resources.Where((p) => Path.GetExtension(p.Item2) is not ".txt"))
        {
            testCases.Add(resourceName, fileName);
        }

        Assert.NotEmpty(testCases);

        return testCases;
    }

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
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath, cancellationToken);
        AssertResource(htmlResponse);

        using var jsResponse = await client.GetAsync(swaggerUijsPath, cancellationToken);
        AssertResource(jsResponse, weakETag: false);

        using var indexCss = await client.GetAsync(indexCssPath, cancellationToken);
        AssertResource(indexCss, weakETag: false);

        using var cssResponse = await client.GetAsync(swaggerUiCssPath, cancellationToken);
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
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync(indexJsPath, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(cancellationToken);

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
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(CustomUIConfig.Startup), outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/swagger/index.js", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains("\"plugins\":[\"customPlugin1\",\"customPlugin2\"]", jsContent);
    }

    [Fact]
    public async Task IndexUrl_Does_Not_Define_Plugins()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/index.js", cancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(cancellationToken);
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
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(CustomUIIndex.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Contains("Example.com", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsInterceptors_IfConfigured()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(CustomUIConfig.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.js", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Contains("\"RequestInterceptorFunction\":", content);
        Assert.Contains("\"ResponseInterceptorFunction\":", content);
    }

    [Theory]
    [InlineData("/swagger/index.html", "/swagger/index.js", new[] { "Version 1.0", "Version 2.0" })]
    [InlineData("/swagger/1.0/index.html", "/swagger/1.0/index.js", new[] { "Version 1.0" })]
    [InlineData("/swagger/2.0/index.html", "/swagger/2.0/index.js", new[] { "Version 2.0" })]
    public async Task SwaggerUIMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string jsUrl, string[] versions)
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(MultipleVersions.Startup), outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlUrl, cancellationToken);
        using var jsResponse = await client.GetAsync(jsUrl, cancellationToken);
        var content = await jsResponse.Content.ReadAsStringAsync(cancellationToken);

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
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath, cancellationToken);
        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);

        var content = await htmlResponse.Content.ReadAsStringAsync(cancellationToken);

        Assert.Contains($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{cssPath}\">", content);
        Assert.Contains($"<script src=\"{scriptBundlePath}\" charset=\"utf-8\">", content);
        Assert.Contains($"<script src=\"{scriptPresetsPath}\" charset=\"utf-8\">", content);
    }

    [Theory]
    [MemberData(nameof(SwaggerUIFiles))]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents_Decompressed(string resourceName, string fileName)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        // Act
        using var response = await client.GetAsync(fileName, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType?.MediaType);
        Assert.Empty(response.Content.Headers.ContentEncoding);

        using var actual = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var expected = typeof(SwaggerUIIntegrationTests).Assembly.GetManifestResourceStream(resourceName);

        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.NotEqual(0, actual.Length);
        Assert.NotEqual(0, expected.Length);

        var actualHash = SHA1.HashData(actual);
        var expectedHash = SHA1.HashData(expected);

        Assert.NotEqual(EmptyStringSha256Hash, Convert.ToBase64String(actualHash));
        Assert.Equal(expectedHash, actualHash);

        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.NotEmpty(response.Headers.ETag.Tag);
        Assert.DoesNotContain(EmptyStringSha256Hash, response.Headers.ETag.Tag);

        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Private);
        Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);

        Assert.Equal(response.Content.Headers.ContentLength, actual.Length);
    }

    [Theory]
    [MemberData(nameof(SwaggerUIFiles))]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents_GZip_Compressed(string resourceName, string fileName)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, fileName);
        request.Headers.AcceptEncoding.Add(new("gzip"));

        // Act
        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content.Headers.ContentType?.MediaType);

        if (Path.GetExtension(fileName) is not ".png")
        {
            Assert.Single(response.Content.Headers.ContentEncoding, "gzip");
        }

        using var actual = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var expected = typeof(SwaggerUIIntegrationTests).Assembly.GetManifestResourceStream(resourceName);

        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.NotEqual(0, actual.Length);
        Assert.NotEqual(0, expected.Length);

        bool wasCompressed = response.Content.Headers.ContentEncoding.SequenceEqual(["gzip"]);
        using var decompressed = wasCompressed ? new GZipStream(actual, CompressionMode.Decompress) : actual;

        Assert.True(
            actual.Length <= expected.Length,
            $"The compressed length ({actual.Length}) was not less or equal to the decompressed length ({expected.Length}).");

        var actualHash = SHA1.HashData(decompressed);
        var expectedHash = SHA1.HashData(expected);

        Assert.NotEqual(EmptyStringSha256Hash, Convert.ToBase64String(actualHash));
        Assert.Equal(expectedHash, actualHash);

        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.NotEmpty(response.Headers.ETag.Tag);
        Assert.DoesNotContain(EmptyStringSha256Hash, response.Headers.ETag.Tag);

        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Private);
        Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);

        Assert.Equal(response.Content.Headers.ContentLength, actual.Length);
    }

    [Theory]
    [MemberData(nameof(SwaggerUIFiles))]
    public async Task SwaggerUIMiddleware_Returns_ExpectedAssetContents_NotModified(string resourceName, string fileName)
    {
        // Arrange
        Assert.NotNull(resourceName);

        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(Basic.Startup), outputHelper);
        using var client = site.BuildClient();

        // Act
        using var uncached = await client.GetAsync(fileName, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, uncached.StatusCode);

        Assert.NotNull(uncached.Headers.ETag);
        Assert.False(uncached.Headers.ETag.IsWeak);
        Assert.NotEmpty(uncached.Headers.ETag.Tag);

        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, fileName);
        request.Headers.IfNoneMatch.Add(new(uncached.Headers.ETag.Tag));

        // Act
        using var cached = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);

        using var stream = await cached.Content.ReadAsStreamAsync(cancellationToken);
        Assert.Equal(0, stream.Length);
    }
}
