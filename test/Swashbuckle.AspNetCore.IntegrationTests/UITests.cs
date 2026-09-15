#if NET10_0_OR_GREATER
using ReDocApp = ReDoc;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public class UITests(PlaywrightFixture fixture) : IClassFixture<PlaywrightFixture>
{
    [Fact]
    public async Task Can_Load_SwaggerUI_Page_And_Make_Request()
    {
        // Arrange
        await using var application = new SwaggerUIFixture();

        // Act and Assert
        await fixture.VerifyPage(application.ServerUrl, async (page) =>
        {
            var operation = await page.WaitForSelectorAsync("text=Searches the collection of products by description key words");
            await operation.ClickAsync();

            var button = await page.WaitForSelectorAsync("text=Try it out");
            await button.ClickAsync();

            button = await page.WaitForSelectorAsync("text=Execute");
            await button.ClickAsync();

            var response = await page.WaitForSelectorAsync(".live-responses-table");
            await response.WaitForSelectorAsync("text=200");
        });
    }

    [Fact]
    public async Task Can_Load_Redoc_Page()
    {
        // Arrange
        await using var application = new RedocFixture();

        // Act and Assert
        await fixture.VerifyPage(application.ServerUrl, async (page) =>
        {
            await page.QuerySelectorAsync("text=/products");
        });
    }

    [Fact]
    public async Task Relative_Document_Url_Does_Not_Inherit_Query_String_From_Page_Url()
    {
        await using var application = new RelativeDocumentUrlFixture();
        var url = new Uri(new Uri(application.ServerUrl), "swagger/index.html?foo=bar");

        await fixture.VerifyPage(url.ToString(), async (page) =>
        {
            var link = await page.WaitForSelectorAsync(".info .url");
            var documentUrl = (await link.TextContentAsync())?.Trim();

            Assert.EndsWith("/swagger/v1.json", documentUrl);
        });
    }

    private sealed class RedocFixture : HttpApplicationFixture<ReDocApp.Program>;

    private sealed class SwaggerUIFixture : HttpApplicationFixture<Basic.Startup>;

    private sealed class RelativeDocumentUrlFixture : HttpApplicationFixture<TopLevelSwaggerDoc.Program>;
}
#endif
