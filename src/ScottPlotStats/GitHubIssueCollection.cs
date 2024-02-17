using System.Text;
using System.Text.Json;

namespace ScottPlotStats;

public class GitHubIssueCollection
{
    private readonly List<GitHubIssue> Issues = [];
    public int Count => Issues.Count;
    public void Add(GitHubIssue issue) => Issues.Add(issue);
    public void AddRange(IEnumerable<GitHubIssue> issues) => Issues.AddRange(issues);
    public GitHubIssue[] GetIssues() => [.. Issues];

    public GitHubIssueCollection()
    {

    }

    public void AddRangeFromJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        foreach (JsonElement el in document.RootElement.EnumerateArray())
        {
            GitHubIssue issue = new(el);
            Issues.Add(issue);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string ToJson()
    {
        return JsonSerializer.Serialize(Issues, JsonOptions);
    }

    public static GitHubIssueCollection FromJson(string json)
    {
        List<GitHubIssue> issues = JsonSerializer.Deserialize<List<GitHubIssue>>(json, JsonOptions)
            ?? throw new NullReferenceException();

        GitHubIssueCollection collection = new();
        collection.AddRange(issues);
        return collection;
    }

    public Dictionary<DateOnly, List<GitHubIssue>> GetIssuesByDay()
    {
        DateTime start = Issues.OrderBy(x => x.DateTimeStart).First().DateTimeStart;
        DateTime end = Issues.OrderBy(x => x.DateTimeEnd).Last().DateTimeEnd;
        DateOnly startDay = DateOnly.FromDateTime(start);
        DateOnly endDay = DateOnly.FromDateTime(end);

        Dictionary<DateOnly, List<GitHubIssue>> issuesByDay = [];
        DateOnly binDay = startDay;
        while (binDay <= endDay)
        {
            issuesByDay[binDay] = [];
            binDay = binDay.AddDays(1);
        }

        foreach (var issue in Issues)
        {
            DateOnly openDay = DateOnly.FromDateTime(issue.DateTimeStart);
            DateOnly closeDay = DateOnly.FromDateTime(issue.DateTimeEnd);
            DateOnly issueDay = openDay;
            while (issueDay <= closeDay)
            {
                issuesByDay[issueDay].Add(issue);
                issueDay = issueDay.AddDays(1);
            }
        }

        return issuesByDay;
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
