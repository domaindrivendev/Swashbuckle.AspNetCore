using System.Net;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.SwaggerUI;

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

        using var response = await client.GetAsync(requestPath, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.Equal(expectedRedirectPath, response.Headers.Location.ToString());
    }

    [Theory]
    [InlineData("Swashbuckle.AspNetCore.SwaggerUI.node_modules")]
    [InlineData("Swashbuckle.AspNetCore.SwaggerUI.node_modules.swagger_ui_dist")]
    public void ResourceRead_CompressedEmbeddedFileProvider(string baseNamespace)
    {
        //confirm that GZipCompressedEmbeddedFileProvider is the same as EmbeddedFileProvider
        var provider = new EmbeddedFileProvider(typeof(SwaggerUIOptions).Assembly, baseNamespace);
        var compressedProvider = new GZipCompressedEmbeddedFileProvider(typeof(SwaggerUIOptions).Assembly, baseNamespace);
        var checkSubpaths = new string[]
        {
            "/",
            null,
            string.Empty,
            " ",
            "\t",
            "\n",
            "/swagger_ui_dist",
            "swagger_ui_dist",
            "/nodir",
            "nodir"
        };

        foreach (var subpath in checkSubpaths)
        {
            AssertResources(provider, compressedProvider, subpath);
        }

        var nonExistentFile = Guid.NewGuid().ToString();
        AssertFileInfo(provider.GetFileInfo(nonExistentFile), compressedProvider.GetFileInfo(nonExistentFile));

        static void AssertResources(IFileProvider expectedProvider, IFileProvider actualProvider, string subpath)
        {
            var expectedContents = expectedProvider.GetDirectoryContents(subpath);
            var actualContents = actualProvider.GetDirectoryContents(subpath);

            Assert.Equal(expectedContents.Exists, actualContents.Exists);
            Assert.Equal(expectedContents.Count(), actualContents.Count());
            var actualResourceMap = actualContents.ToDictionary(m => m.Name);

            foreach (var expectedFileInfo in expectedContents)
            {
                Assert.True(actualResourceMap.TryGetValue(expectedFileInfo.Name, out var actualFileInfo));
                Assert.NotNull(actualFileInfo);
                Assert.True(actualFileInfo.Exists);
                AssertFileInfo(expectedFileInfo, actualFileInfo);
                AssertFileInfo(expectedProvider.GetFileInfo(expectedFileInfo.Name), actualProvider.GetFileInfo(expectedFileInfo.Name));
            }
        }

        static void AssertFileInfo(IFileInfo expectedFileInfo, IFileInfo actualFileInfo)
        {
            Assert.Equal(expectedFileInfo.Exists, actualFileInfo.Exists);
            Assert.Equal(expectedFileInfo.IsDirectory, actualFileInfo.IsDirectory);
            Assert.Equal(expectedFileInfo.LastModified, actualFileInfo.LastModified);
            Assert.Equal(expectedFileInfo.PhysicalPath, actualFileInfo.PhysicalPath);

            if (expectedFileInfo.Exists && !expectedFileInfo.IsDirectory)
            {
                Assert.True(actualFileInfo.Length > 0);

                using var stream = actualFileInfo.CreateReadStream();
                Assert.NotNull(stream);
                var buffer = new byte[256];
                var readLength = stream.Read(buffer, 0, buffer.Length);
                Assert.True(readLength > 0);
                //we can check for correctness here
            }
            else
            {
                Assert.Equal(expectedFileInfo.Length, actualFileInfo.Length);
                Assert.ThrowsAny<FileNotFoundException>(() => expectedFileInfo.CreateReadStream());
                Assert.ThrowsAny<FileNotFoundException>(() => actualFileInfo.CreateReadStream());
            }
        }
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

        using var htmlResponse = await client.GetAsync(htmlPath, TestContext.Current.CancellationToken);
        AssertResource(htmlResponse);

        using var jsResponse = await client.GetAsync(swaggerUijsPath, TestContext.Current.CancellationToken);
        AssertResource(jsResponse);

        using var indexCss = await client.GetAsync(indexCssPath, TestContext.Current.CancellationToken);
        AssertResource(indexCss);

        using var cssResponse = await client.GetAsync(swaggerUiCssPath, TestContext.Current.CancellationToken);
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
        var site = new TestSite(typeof(CustomUIConfig.Startup));
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/swagger/index.js", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);

        var jsContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("\"plugins\":[\"customPlugin1\",\"customPlugin2\"]", jsContent);
    }

    [Fact]
    public async Task IndexUrl_DoesntDefinePlugins()
    {
        var site = new TestSite(typeof(Basic.Startup));
        using var client = site.BuildClient();

        using var jsResponse = await client.GetAsync("/index.js", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, jsResponse.StatusCode);
        var jsContent = await jsResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("\"plugins\"", jsContent);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomPageTitleAndStylesheets_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("<title>CustomUIConfig</title>", content);
        Assert.Contains("<link href='/ext/custom-stylesheet.css' rel='stylesheet' media='screen' type='text/css' />", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsCustomIndexHtml_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIIndex.Startup));
        using var client = site.BuildClient();

        using var response = await client.GetAsync("/swagger/index.html", TestContext.Current.CancellationToken);
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("Example.com", content);
    }

    [Fact]
    public async Task IndexUrl_ReturnsInterceptors_IfConfigured()
    {
        var site = new TestSite(typeof(CustomUIConfig.Startup));
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
        var site = new TestSite(typeof(MultipleVersions.Startup));
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
        var site = new TestSite(startupType);
        using var client = site.BuildClient();

        using var htmlResponse = await client.GetAsync(htmlPath, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, htmlResponse.StatusCode);

        var content = await htmlResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{cssPath}\">", content);
        Assert.Contains($"<script src=\"{scriptBundlePath}\" charset=\"utf-8\">", content);
        Assert.Contains($"<script src=\"{scriptPresetsPath}\" charset=\"utf-8\">", content);
    }
}
