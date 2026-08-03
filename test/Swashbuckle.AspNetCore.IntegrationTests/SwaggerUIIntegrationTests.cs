using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class SwaggerUIIntegrationTests(ITestOutputHelper outputHelper)
{
    private const string CssAsset = "/swagger/swagger-ui.css";
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

        var filtered = resources
            .Where((p) => Path.GetFileName(p.Item2) is not "oauth2-redirect.html")
            .Where((p) => Path.GetExtension(p.Item2) is not ".txt");

        foreach (var (resourceName, fileName) in filtered)
        {
            testCases.Add(resourceName, fileName);
        }

        Assert.NotEmpty(testCases);

        return testCases;
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "/", "index.html")]
    [InlineData(typeof(Basic.StartupWithAbsoluteRoutePrefix), "/abs", "abs/index.html")]
    [InlineData(typeof(Basic.StartupWithRelativeRoutePrefix), "/rel", "rel/index.html")]
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
    [InlineData(typeof(Basic.Startup), "/index.html")]
    [InlineData(typeof(Basic.StartupWithAbsoluteRoutePrefix), "/abs/index.html")]
    [InlineData(typeof(Basic.StartupWithRelativeRoutePrefix), "/rel/index.html")]
    [InlineData(typeof(CustomUIConfig.Startup), "/swagger/index.html")]
    public async Task IndexUrl_HeadRequest_ReturnsMetadata(
        Type startupType,
        string requestPath)
    {
        var site = new TestSite(startupType, outputHelper);
        using var client = site.BuildClient();
        using var request = new HttpRequestMessage(HttpMethod.Head, requestPath);
        using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Content.Headers.ContentLength > 0, "Content-Length should not be be 0.");
        Assert.Empty(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(typeof(Basic.Startup), "/index.html", "/swagger-ui.js", "/index.css", "/swagger-ui.css")]
    [InlineData(typeof(Basic.StartupWithAbsoluteRoutePrefix), "/abs/index.html", "/abs/swagger-ui.js", "/abs/index.css", "/abs/swagger-ui.css")]
    [InlineData(typeof(Basic.StartupWithRelativeRoutePrefix), "/rel/index.html", "/rel/swagger-ui.js", "/rel/index.css", "/rel/swagger-ui.css")]
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
        AssertResource(jsResponse);

        using var indexCss = await client.GetAsync(indexCssPath, cancellationToken);
        AssertResource(indexCss);

        using var cssResponse = await client.GetAsync(swaggerUiCssPath, cancellationToken);
        AssertResource(cssResponse);

        static void AssertResource(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.False(response.Headers.ETag.IsWeak);
            Assert.NotEmpty(response.Headers.ETag.Tag);
            Assert.NotNull(response.Headers.CacheControl);
            Assert.True(response.Headers.CacheControl.Private);
            Assert.Equal(TimeSpan.Zero, response.Headers.CacheControl.MaxAge);
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

        using var response = await client.GetAsync(indexJsPath, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var jsContent = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Contains("SwaggerUIBundle", jsContent);
        Assert.DoesNotContain("%(DocumentTitle)", jsContent);
        Assert.DoesNotContain("%(HeadContent)", jsContent);
        Assert.DoesNotContain("%(StylesPath)", jsContent);
        Assert.DoesNotContain("%(ScriptBundlePath)", jsContent);
        Assert.DoesNotContain("%(ScriptPresetsPath)", jsContent);
        Assert.DoesNotContain("%(ConfigObject)", jsContent);
        Assert.DoesNotContain("%(OAuthConfigObject)", jsContent);
        Assert.DoesNotContain("%(Interceptors)", jsContent);

        using var request = new HttpRequestMessage(HttpMethod.Get, indexJsPath);
        request.Headers.IfNoneMatch.Add(response.Headers.ETag);

        using var cached = await client.SendAsync(request, cancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);

        using var stream = await cached.Content.ReadAsStreamAsync(cancellationToken);
        Assert.Equal(0, stream.Length);
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
        Assert.Equal(TimeSpan.Zero, response.Headers.CacheControl.MaxAge);

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
        Assert.Equal(TimeSpan.Zero, response.Headers.CacheControl.MaxAge);

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
        request.Headers.IfNoneMatch.Add(uncached.Headers.ETag);

        // Act
        using var cached = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);

        using var stream = await cached.Content.ReadAsStreamAsync(cancellationToken);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task DocumentUrlsEndpoint_ReturnsJsonWithCacheHeaders()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(MultipleVersions.Startup), outputHelper);
        using var client = site.BuildClient();

        var requestUri = "/swagger/documentUrls";

        // Act
        using var response = await client.GetAsync(requestUri, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/javascript; charset=utf-8", response.Content.Headers.ContentType.ToString());

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.NotEmpty(content);
        Assert.Contains("Version 1.0", content);
        Assert.Contains("Version 2.0", content);

        // Verify cache headers are set
        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.NotEmpty(response.Headers.ETag.Tag);

        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Private);
        Assert.Equal(TimeSpan.Zero, response.Headers.CacheControl.MaxAge);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.IfNoneMatch.Add(response.Headers.ETag);

        using var cached = await client.SendAsync(request, cancellationToken);

        Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);

        using var stream = await cached.Content.ReadAsStreamAsync(cancellationToken);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Uses_A_Distinct_ETag_Per_Representation()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        using var identityRequest = new HttpRequestMessage(HttpMethod.Get, CssAsset);

        using var gzipRequest = new HttpRequestMessage(HttpMethod.Get, CssAsset);
        gzipRequest.Headers.AcceptEncoding.Add(new("gzip"));

        // Act
        using var identity = await client.SendAsync(identityRequest, cancellationToken);
        using var gzip = await client.SendAsync(gzipRequest, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, identity.StatusCode);
        Assert.Equal(HttpStatusCode.OK, gzip.StatusCode);

        var identityBody = await identity.Content.ReadAsByteArrayAsync(cancellationToken);
        var gzipBody = await gzip.Content.ReadAsByteArrayAsync(cancellationToken);

        outputHelper.WriteLine($"identity: {identityBody.Length} bytes, ETag {identity.Headers.ETag}");
        outputHelper.WriteLine($"gzip:     {gzipBody.Length} bytes, ETag {gzip.Headers.ETag}");

        Assert.Empty(identity.Content.Headers.ContentEncoding);
        Assert.Equal(["gzip"], [.. gzip.Content.Headers.ContentEncoding]);
        Assert.NotEqual(identityBody.Length, gzipBody.Length);

        Assert.NotNull(identity.Headers.ETag);
        Assert.NotNull(gzip.Headers.ETag);
        Assert.NotEqual(identity.Headers.ETag, gzip.Headers.ETag);

        Assert.Equal(["Accept-Encoding"], [.. identity.Headers.Vary]);
        Assert.Equal(["Accept-Encoding"], [.. gzip.Headers.Vary]);

        Assert.Equal($"\"{Convert.ToBase64String(SHA1.HashData(identityBody))}\"", identity.Headers.ETag.ToString());
        Assert.Equal($"\"{Convert.ToBase64String(SHA1.HashData(gzipBody))}\"", gzip.Headers.ETag.ToString());
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Does_Not_Return_NotModified_For_A_Representation_The_Client_Never_Received()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        using var identity = await client.GetAsync(CssAsset, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, identity.StatusCode);
        Assert.Empty(identity.Content.Headers.ContentEncoding);

        var etag = identity.Headers.ETag;
        Assert.NotNull(etag);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, CssAsset);
        request.Headers.IfNoneMatch.Add(etag);
        request.Headers.AcceptEncoding.Add(new("gzip"));

        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["gzip"], [.. response.Content.Headers.ContentEncoding]);
        Assert.NotEqual(etag, response.Headers.ETag);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Returns_NotModified_For_The_Representation_The_Client_Holds()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        using var firstRequest = new HttpRequestMessage(HttpMethod.Get, CssAsset);
        firstRequest.Headers.AcceptEncoding.Add(new("gzip"));

        using var first = await client.SendAsync(firstRequest, cancellationToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.NotNull(first.Headers.ETag);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, CssAsset);
        request.Headers.IfNoneMatch.Add(first.Headers.ETag);
        request.Headers.AcceptEncoding.Add(new("gzip"));

        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotModified, response.StatusCode);
        Assert.Equal(["Accept-Encoding"], [.. response.Headers.Vary]);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_DocumentUrlsPath_Is_Treated_As_A_Literal_Path()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI((options) =>
        {
            options.ExposeSwaggerDocumentUrlsRoute = true;
            options.SwaggerDocumentUrlsPath = ".*";
        }));

        using var client = server.CreateClient();

        // Act
        using var shadowed = await client.GetAsync("/swagger/v1/swagger.json", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, shadowed.StatusCode);

        using var literal = await client.GetAsync("/swagger/.*", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, literal.StatusCode);
        Assert.Equal("application/javascript", literal.Content.Headers.ContentType?.MediaType);

        var body = await literal.Content.ReadAsStringAsync(cancellationToken);
        outputHelper.WriteLine(body);

        Assert.StartsWith("[", body);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_DocumentUrlsPath_Containing_Regex_Metacharacters_Does_Not_Fault()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI((options) =>
        {
            options.ExposeSwaggerDocumentUrlsRoute = true;
            options.SwaggerDocumentUrlsPath = "urls(";
        }));

        using var client = server.CreateClient();

        // Act
        using var unrelated = await client.GetAsync("/some/unrelated/path", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, unrelated.StatusCode);

        using var literal = await client.GetAsync("/swagger/urls(", cancellationToken);

        Assert.Equal(HttpStatusCode.OK, literal.StatusCode);
        Assert.Equal("application/javascript", literal.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Does_Not_Serve_Html_For_Paths_That_Are_Not_index_html()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        // Act
        using var response = await client.GetAsync("/swagger/indexXjs", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("/swagger/index.html", "text/html", "<div id=\"swagger-ui\"></div>")]
    [InlineData("/swagger/INDEX.HTML", "text/html", "<div id=\"swagger-ui\"></div>")]
    [InlineData("/swagger/index.js", "application/javascript", "var configObject")]
    [InlineData("/swagger/INDEX.JS", "application/javascript", "var configObject")]
    public async Task SwaggerUIMiddleware_Selects_The_File_Case_Insensitively(
        string requestPath,
        string expectedMediaType,
        string expectedContent)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        // Act
        using var response = await client.GetAsync(requestPath, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedMediaType, response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        Assert.Contains(expectedContent, body);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("TRACE")]
    public async Task SwaggerUIMiddleware_Does_Not_Serve_Assets_For_Unsupported_Http_Methods(string method)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(new(method), CssAsset);

        // Act
        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    public async Task SwaggerUIMiddleware_Still_Serves_Assets_For_Supported_Http_Methods(string method)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        using var request = new HttpRequestMessage(new(method), CssAsset);

        // Act
        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Content.Headers.ContentLength > 0);
    }

    [Theory]
    [InlineData("/swagger.swagger-ui.css")]
    [InlineData("/swaggerXswagger-ui.css")]
    public async Task SwaggerUIMiddleware_Requires_A_Segment_Boundary_After_The_Route_Prefix(string requestPath)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI());
        using var client = server.CreateClient();

        // Act
        using var response = await client.GetAsync(requestPath, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("swagger")]
    [InlineData("a/nested/prefix")]
    public async Task SwaggerUIMiddleware_Still_Serves_Assets_Below_The_Route_Prefix(string routePrefix)
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI((options) => options.RoutePrefix = routePrefix));
        using var client = server.CreateClient();

        var requestPath = string.IsNullOrEmpty(routePrefix) ? "/swagger-ui.css" : $"/{routePrefix}/swagger-ui.css";

        // Act
        using var response = await client.GetAsync(requestPath, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_Encodes_DocumentTitle()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI(
            (options) => options.DocumentTitle = "</title><script>alert(1)</script>"));

        using var client = server.CreateClient();

        // Act
        using var response = await client.GetAsync("/swagger/index.html", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        outputHelper.WriteLine(body);

        Assert.DoesNotContain("<title></title><script>alert(1)</script></title>", body);
        Assert.Contains("<title>&lt;/title&gt;&lt;script&gt;alert(1)&lt;/script&gt;</title>", body);
    }

    [Fact]
    public async Task SwaggerUIMiddleware_ConfigObject_Is_Not_Spliced_Into_A_String_Literal()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        using var server = TestSite.CreateServer((app) => app.UseSwaggerUI((options) =>
        {
            options.ConfigObject.Urls = [new() { Url = "v1/swagger.json", Name = "x');alert(1);//" }];
            options.JsonSerializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        }));

        using var client = server.CreateClient();

        // Act
        using var response = await client.GetAsync("/swagger/index.js", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        const string Prefix = "var configObject =";

        var line = body.Split('\n').First((p) => p.Contains(Prefix));
        outputHelper.WriteLine(line);

        Assert.DoesNotContain("JSON.parse('", body);

        var json = line[(line.IndexOf(Prefix, StringComparison.Ordinal) + Prefix.Length)..].Trim().TrimEnd(';');

        using var config = JsonDocument.Parse(json);

        Assert.Equal("x');alert(1);//", config.RootElement.GetProperty("urls")[0].GetProperty("name").GetString());
    }
}
