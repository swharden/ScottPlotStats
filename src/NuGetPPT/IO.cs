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
        public static Dictionary<string, int> FromJson(string json)
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return document.RootElement.GetProperty("records")
                .EnumerateObject()
                .ToDictionary(x => x.Name, x => int.Parse(x.Value.ToString()));
        }

        /// <summary>
        /// Create JSON from a dictionary of timestamp:downloads
        /// </summary>
        public static string ToJson(Dictionary<string, int> downloads, string packageName)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            writer.WriteString("package", packageName);
            writer.WriteStartObject("records");
            foreach (string timestamp in downloads.Keys)
                writer.WriteNumber(timestamp, downloads[timestamp]);
            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}