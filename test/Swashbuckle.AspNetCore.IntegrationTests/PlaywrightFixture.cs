using Microsoft.Playwright;

namespace Swashbuckle.AspNetCore.IntegrationTests;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    private bool _installed;

    public ValueTask InitializeAsync()
    {
        EnsureInstalled();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public async Task VerifyPage(string url, Func<IPage, Task> test)
    {
        EnsureInstalled();

        using var playwright = await Playwright.CreateAsync();
        var browserType = playwright[BrowserType.Chromium];

        var options = new BrowserTypeLaunchOptions();

        if (System.Diagnostics.Debugger.IsAttached)
        {
            options.Args = ["--auto-open-devtools-for-tabs"];
            options.Headless = false;
            options.SlowMo = 100;
        }

        await using var browser = await playwright[BrowserType.Chromium].LaunchAsync(options);
        await using var context = await browser.NewContextAsync();

        var page = await context.NewPageAsync();

        await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });

        await test(page);
    }

    private void EnsureInstalled()
    {
        if (!_installed)
        {
            int result = Microsoft.Playwright.Program.Main(["install", "chromium", "--only-shell", "--with-deps"]);

            if (result != 0)
            {
                throw new InvalidOperationException($"Failed to install Playwright dependencies: {result}.");
            }

            _installed = true;
        }
    }
}
