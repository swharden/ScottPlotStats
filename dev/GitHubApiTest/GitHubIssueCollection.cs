using System.Text.Json;

namespace GitHubApiTest;

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

    public void AddRangeFromJson(string body)
    {
        using JsonDocument document = JsonDocument.Parse(body);
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
}
