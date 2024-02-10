using System.Diagnostics;
using System.Text.Json;

namespace ScottPlotStats;

public static class NuGetAPI
{
    public static int GetDownloadCount(string packageName)
    {
        // TODO: try/catch?
        Stopwatch sw1 = Stopwatch.StartNew();
        int primary = GetDownloadCount(packageName, true);
        Console.WriteLine($"Primary: {primary:N0} ({sw1.Elapsed.TotalMilliseconds:N0} ms)");

        Stopwatch sw2 = Stopwatch.StartNew();
        int secondary = GetDownloadCount(packageName, false);
        Console.WriteLine($"Secondary: {secondary:N0} ({sw2.Elapsed.TotalMilliseconds:N0} ms)");

        return Math.Max(primary, secondary);
    }

    private static int GetDownloadCount(string packageName, bool primary)
    {
        string urlBase = primary
            ? "https://azuresearch-usnc.nuget.org/query"
            : "https://azuresearch-ussc.nuget.org/query";

        string url = $"{urlBase}?take=1&q={packageName}";

        JsonDocument doc = RequestJson(url);
        return doc
            .RootElement
            .GetProperty("data")
            .EnumerateArray()
            .First()
            .GetProperty("totalDownloads")
            .GetInt32();
    }

    private static JsonDocument RequestJson(string url) => RequestJsonAsync(url).Result;

    private static async Task<JsonDocument> RequestJsonAsync(string url)
    {
        using HttpClient client = new();
        using HttpResponseMessage response = await client.GetAsync(url);
        using HttpContent content = response.Content;
        string json = await content.ReadAsStringAsync();
        return JsonDocument.Parse(json);
    }
}
