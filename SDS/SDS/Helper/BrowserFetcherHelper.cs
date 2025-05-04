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