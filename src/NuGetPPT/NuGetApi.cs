using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NuGetPPT
{
    public static class NuGetApi
    {
        public static int ReadTotalDownloads(string packageJson)
        {
            using JsonDocument document = JsonDocument.Parse(packageJson);
            return document.RootElement.GetProperty("data").EnumerateArray().First().GetProperty("totalDownloads").GetInt32();
        }

        public static string RequestJson(string packageName)
        {
            using WebClient client = new();
            string url = "https://azuresearch-usnc.nuget.org/query?take=1&q=" + packageName;
            return client.DownloadString(url);
        }
    }
}
