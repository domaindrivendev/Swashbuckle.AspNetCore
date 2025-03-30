using System.Net;
using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.ReDoc;
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

[Collection("TestSite")]
public class ReDocIntegrationTests
{
    [Fact]
    public async Task RoutePrefix_RedirectsToIndexUrl()
    {
        var site = new TestSite(typeof(ReDocApp.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/api-docs");

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal("api-docs/index.html", response.Headers.Location.ToString());
    }

    [Fact]
    public async Task IndexUrl_ReturnsEmbeddedVersionOfTheRedocUI()
    {
        var site = new TestSite(typeof(ReDocApp.Startup));
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync("/api-docs/index.html");
        using var cssResponse = await client.GetAsync("/api-docs/index.css");
        using var jsResponse = await client.GetAsync("/api-docs/redoc.standalone.js");

        AssertResource(htmlResponse);
        AssertResource(cssResponse);
        AssertResource(jsResponse);

        static void AssertResource(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Headers.ETag);
            Assert.True(response.Headers.ETag.IsWeak);
            Assert.NotEmpty(response.Headers.ETag.Tag);
            Assert.NotNull(response.Headers.CacheControl);
            Assert.True(response.Headers.CacheControl.Public);
            Assert.Equal(TimeSpan.FromDays(7), response.Headers.CacheControl.MaxAge);
        }
    }

    [Fact]
    public async Task RedocMiddleware_ReturnsInitializerScript()
    {
        var site = new TestSite(typeof(ReDocApp.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/api-docs/index.js");
        var content = await response.Content.ReadAsStringAsync();

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
        var site = new TestSite(typeof(ReDocApp.Startup));
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync("/Api-Docs/index.html");
        using var cssResponse = await client.GetAsync("/Api-Docs/index.css");
        using var jsInitResponse = await client.GetAsync("/Api-Docs/index.js");
        using var jsRedocResponse = await client.GetAsync("/Api-Docs/redoc.standalone.js");

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
        var site = new TestSite(typeof(MultipleVersions.Startup));
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlUrl);
        using var jsResponse = await client.GetAsync(jsUrl);
        var content = await jsResponse.Content.ReadAsStringAsync();

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
}
