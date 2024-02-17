using ScottPlot;
using System.Text;
using System.Text.Json;

namespace ScottPlotStats;

public class GitHubStarsCollection
{
    private readonly List<GitHubStar> Stars = [];
    public int Count => Stars.Count;
    public GitHubStar[] GetStars() => Stars.ToArray();

    public void AddRange(IEnumerable<GitHubStar> stars) => Stars.AddRange(stars);

    public void AddRangeFromJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        foreach (JsonElement el in document.RootElement.EnumerateArray())
        {
            string user = el.GetProperty("user").GetProperty("login").GetString() ?? string.Empty;
            DateTime dt = DateTime.Parse(el.GetProperty("starred_at").GetString() ?? string.Empty).ToUniversalTime();
            GitHubStar issue = new(user, dt);
            Stars.Add(issue);
        }
    }

    public DateTime[] GetStarDates()
    {
        return Stars
            .OrderBy(x => x.DateTime)
            .Select(x => x.DateTime)
            .ToArray();
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string ToJson()
    {
        return JsonSerializer.Serialize(Stars, JsonOptions);
    }

    public static GitHubStarsCollection FromJson(string json)
    {
        GitHubStarsCollection collection = new();

        List<GitHubStar> stars = JsonSerializer.Deserialize<List<GitHubStar>>(json, JsonOptions)
            ?? throw new NullReferenceException();

        collection.AddRange(stars);

        return collection;
    }

    public string GetLogJson()
    {
        using MemoryStream ms1 = new();
        JsonWriterOptions options = new() { Indented = true };
        using Utf8JsonWriter writer = new(ms1, options);
        writer.WriteStartObject();
        writer.WriteString("updated", DateTime.Now.ToUniversalTime().ToString("o"));
        writer.WriteNumber("total", Count);
        writer.WriteEndObject();
        writer.Flush();
        writer.Flush();
        byte[] bytes = ms1.ToArray();
        return Encoding.UTF8.GetString(bytes);
    }
}
