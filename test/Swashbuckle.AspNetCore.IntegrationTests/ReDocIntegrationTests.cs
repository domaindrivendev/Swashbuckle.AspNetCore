using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.ReDoc;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class ReDocIntegrationTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task RoutePrefix_RedirectsToIndexUrl()
    {
        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/api-docs", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("api-docs/index.html", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task IndexUrl_ReturnsEmbeddedVersionOfTheRedocUI()
    {
        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync("/api-docs/index.html", TestContext.Current.CancellationToken);
        using var cssResponse = await client.GetAsync("/api-docs/index.css", TestContext.Current.CancellationToken);
        using var jsResponse = await client.GetAsync("/api-docs/redoc.standalone.js", TestContext.Current.CancellationToken);

        AssertResource(htmlResponse);
        AssertResource(cssResponse);
        AssertResource(jsResponse, weakETag: false);

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

    [Fact]
    public async Task RedocMiddleware_ReturnsInitializerScript()
    {
        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/api-docs/index.js", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Redoc.init", content);
        Assert.DoesNotContain("%(DocumentTitle)", content);
        Assert.DoesNotContain("%(HeadContent)", content);
        Assert.DoesNotContain("%(SpecUrl)", content);
        Assert.DoesNotContain("%(ConfigObject)", content);
    }

    [Fact]
    public async Task IndexUrl_IgnoresUrlCase()
    {
        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync("/Api-Docs/index.html", TestContext.Current.CancellationToken);
        using var cssResponse = await client.GetAsync("/Api-Docs/index.css", TestContext.Current.CancellationToken);
        using var jsInitResponse = await client.GetAsync("/Api-Docs/index.js", TestContext.Current.CancellationToken);
        using var jsRedocResponse = await client.GetAsync("/Api-Docs/redoc.standalone.js", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, jsInitResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, jsRedocResponse.StatusCode);
    }

    [Theory]
    [InlineData("/redoc/1.0/index.html", "/redoc/1.0/index.js", "/swagger/1.0/swagger.json")]
    [InlineData("/redoc/2.0/index.html", "/redoc/2.0/index.js", "/swagger/2.0/swagger.json")]
    public async Task RedocMiddleware_CanBeConfiguredMultipleTimes(string htmlUrl, string jsUrl, string swaggerPath)
    {
        var site = new TestSite(typeof(MultipleVersions.Startup), outputHelper);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlUrl, TestContext.Current.CancellationToken);
        using var jsResponse = await client.GetAsync(jsUrl, TestContext.Current.CancellationToken);
        var content = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        Assert.Contains(swaggerPath, content);
    }

    [Fact]
    public void ReDocOptions_Extensions()
    {
        // Arrange
        var options = new ReDocOptions();

        // Act and Assert
        Assert.NotNull(options.IndexStream);
        Assert.Null(options.JsonSerializerOptions);
        Assert.Null(options.SpecUrl);
        Assert.Equal("API Docs", options.DocumentTitle);
        Assert.Equal(string.Empty, options.HeadContent);
        Assert.Equal("api-docs", options.RoutePrefix);

        Assert.NotNull(options.ConfigObject);
        Assert.NotNull(options.ConfigObject.AdditionalItems);
        Assert.Empty(options.ConfigObject.AdditionalItems);
        Assert.Null(options.ConfigObject.ScrollYOffset);
        Assert.Equal("all", options.ConfigObject.ExpandResponses);
        Assert.False(options.ConfigObject.DisableSearch);
        Assert.False(options.ConfigObject.HideDownloadButton);
        Assert.False(options.ConfigObject.HideHostname);
        Assert.False(options.ConfigObject.HideLoading);
        Assert.False(options.ConfigObject.NativeScrollbars);
        Assert.False(options.ConfigObject.NoAutoAuth);
        Assert.False(options.ConfigObject.OnlyRequiredInSamples);
        Assert.False(options.ConfigObject.PathInMiddlePanel);
        Assert.False(options.ConfigObject.RequiredPropsFirst);
        Assert.False(options.ConfigObject.SortPropsAlphabetically);
        Assert.False(options.ConfigObject.UntrustedSpec);

        // Act
        options.DisableSearch();
        options.EnableUntrustedSpec();
        options.ExpandResponses("response");
        options.HideDownloadButton();
        options.HideHostname();
        options.HideLoading();
        options.InjectStylesheet("custom.css", "screen and (max-width: 700px)");
        options.NativeScrollbars();
        options.NoAutoAuth();
        options.OnlyRequiredInSamples();
        options.PathInMiddlePanel();
        options.RequiredPropsFirst();
        options.ScrollYOffset(42);
        options.SortPropsAlphabetically();
        options.SpecUrl("spec.json");

        // Assert
        Assert.Equal("<link href='custom.css' rel='stylesheet' media='screen and (max-width: 700px)' type='text/css' />" + Environment.NewLine, options.HeadContent);
        Assert.Equal("spec.json", options.SpecUrl);
        Assert.Equal("response", options.ConfigObject.ExpandResponses);
        Assert.Equal(42, options.ConfigObject.ScrollYOffset);
        Assert.True(options.ConfigObject.DisableSearch);
        Assert.True(options.ConfigObject.HideDownloadButton);
        Assert.True(options.ConfigObject.HideHostname);
        Assert.True(options.ConfigObject.HideLoading);
        Assert.True(options.ConfigObject.NativeScrollbars);
        Assert.True(options.ConfigObject.NoAutoAuth);
        Assert.True(options.ConfigObject.OnlyRequiredInSamples);
        Assert.True(options.ConfigObject.PathInMiddlePanel);
        Assert.True(options.ConfigObject.RequiredPropsFirst);
        Assert.True(options.ConfigObject.SortPropsAlphabetically);
        Assert.True(options.ConfigObject.UntrustedSpec);
    }

    [Fact]
    public async Task ReDocMiddleware_Returns_ExpectedAssetContents_Decompressed()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        // Act
        using var response = await client.GetAsync("/Api-Docs/redoc.standalone.js", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/javascript", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal([], response.Content.Headers.ContentEncoding);

        using var actual = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var expected = typeof(ReDocIntegrationTests).Assembly.GetManifestResourceStream("Swashbuckle.AspNetCore.IntegrationTests.Embedded.ReDoc.redoc.standalone.js");

        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.NotEqual(0, actual.Length);
        Assert.NotEqual(0, expected.Length);

        var actualHash = SHA1.HashData(actual);
        var expectedHash = SHA1.HashData(expected);

        Assert.NotEqual("2jmj7l5rSw0yVb/vlWAYkK/YBwk=", Convert.ToBase64String(actualHash));
        Assert.Equal(expectedHash, actualHash);

        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.NotEmpty(response.Headers.ETag.Tag);

        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Private);
        Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);
    }

    [Fact]
    public async Task ReDocMiddleware_Returns_ExpectedAssetContents_GZip_Compressed()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        var site = new TestSite(typeof(ReDocApp.Startup), outputHelper);
        using var client = site.BuildClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/Api-Docs/redoc.standalone.js");
        request.Headers.AcceptEncoding.Add(new("gzip"));

        // Act
        using var response = await client.SendAsync(request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/javascript", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(["gzip"], [.. response.Content.Headers.ContentEncoding]);

        using var actual = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var expected = typeof(ReDocIntegrationTests).Assembly.GetManifestResourceStream("Swashbuckle.AspNetCore.IntegrationTests.Embedded.ReDoc.redoc.standalone.js");

        Assert.NotNull(actual);
        Assert.NotNull(expected);

        Assert.NotEqual(0, actual.Length);
        Assert.NotEqual(0, expected.Length);

        using var decompressed = new GZipStream(actual, CompressionMode.Decompress);

        Assert.True(
            actual.Length < expected.Length,
            $"The compressed length ({actual.Length}) was not less than the decompressed length ({expected.Length}).");

        var actualHash = SHA1.HashData(decompressed);
        var expectedHash = SHA1.HashData(expected);

        Assert.NotEqual("2jmj7l5rSw0yVb/vlWAYkK/YBwk=", Convert.ToBase64String(actualHash));
        Assert.Equal(expectedHash, actualHash);

        Assert.NotNull(response.Headers.ETag);
        Assert.False(response.Headers.ETag.IsWeak);
        Assert.NotEmpty(response.Headers.ETag.Tag);

        Assert.NotNull(response.Headers.CacheControl);
        Assert.True(response.Headers.CacheControl.Private);
        Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);
    }
}
