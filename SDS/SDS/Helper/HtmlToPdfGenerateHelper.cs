using PuppeteerSharp;

namespace SDS.Helper;

public static class BrowserFetcherHelper
{
    private static readonly BrowserFetcher BrowserFetcher = new BrowserFetcher();

    /// <summary>
    /// Ensures the browser is downloaded and returns the executable path.
    /// </summary>
    public static async Task<string> GetExecutablePathAsync()
    {
        // Get installed browsers
        var installedBrowsers = BrowserFetcher.GetInstalledBrowsers();

        // Try to get Chrome from installed
        var chrome = installedBrowsers.FirstOrDefault(d => d.Browser == SupportedBrowser.Chrome);

        if (chrome == null)
        {
            // If not installed, download latest stable
            var revisionInfo = await BrowserFetcher.DownloadAsync(BrowserTag.Stable);
            return revisionInfo.GetExecutablePath();
        }
        else
        {
            return chrome.GetExecutablePath();
        }
    }
}

public static class HtmlToPdfGenerateHelper
{
    public static async Task<byte[]> GenerateAsync(string url, PdfOptions pdfOptions)
    {
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = await BrowserFetcherHelper.GetExecutablePathAsync(),
        });
        await using var page = await browser.NewPageAsync();

        await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

        return await page.PdfDataAsync(pdfOptions);
    }
}