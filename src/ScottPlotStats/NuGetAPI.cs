using System.Text.Json;

namespace ScottPlotStats;

public static class NuGetAPI
{
    public static CountRecord GetPrimaryCount(string packageName = "scottplot")
    {
        // URL from https://api.nuget.org/v3/index.json
        int count = GetDownloadCount("https://azuresearch-usnc.nuget.org/query", packageName);
        return new CountRecord(DateTime.UtcNow.ToUniversalTime(), 1, count);
    }

    public static CountRecord GetSecondaryCount(string packageName = "scottplot")
    {
        // URL from https://api.nuget.org/v3/index.json
        int count = GetDownloadCount("https://azuresearch-ussc.nuget.org/query", packageName);
        return new CountRecord(DateTime.UtcNow.ToUniversalTime(), 2, count);
    }

    private static int GetDownloadCount(string baseUrl, string packageName)
    {
        string url = $"{baseUrl}?take=1&q={packageName}";
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
