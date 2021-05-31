using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace NuGetPPT
{
    public static class IO
    {
        /// <summary>
        /// Create a dictionary of timestamp:downloads from JSON
        /// </summary>
        public static List<DownloadRecord> FromJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("records")
                .EnumerateObject()
                .Select(x=>new DownloadRecord(x.Name, int.Parse(x.Value.ToString())))
                .ToList();
        }

        /// <summary>
        /// Create JSON from a dictionary of timestamp:downloads
        /// </summary>
        public static string ToJson(List<DownloadRecord> downloads, string packageName)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            writer.WriteString("package", packageName);
            writer.WriteStartObject("records");
            foreach (var d in downloads)
                writer.WriteNumber(d.Timestamp, d.Downloads);
            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}