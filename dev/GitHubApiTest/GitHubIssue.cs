using System.Text.Json;

namespace GitHubApiTest;

public class GitHubIssue
{
    public int Number { get; set; }
    public string Title { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public DateTime DateTimeStart { get; set; }
    public DateTime DateTimeEnd { get; set; }
    public bool IsClosed { get; set; }
    public double OpenDays { get; set; }

    public GitHubIssue()
    {

    }

    public GitHubIssue(JsonElement el)
    {
        Number = el.GetProperty("number").GetInt32();
        Title = el.GetProperty("title").GetString() ?? string.Empty;
        User = el.GetProperty("user").GetProperty("login").GetString() ?? string.Empty;
        DateTimeStart = DateTime.Parse(el.GetProperty("created_at").GetString() ?? string.Empty).ToUniversalTime();
        State = el.GetProperty("state").GetString() ?? string.Empty;
        IsClosed = State == "closed";
        DateTimeEnd = IsClosed
            ? DateTime.Parse(el.GetProperty("closed_at").GetString() ?? string.Empty).ToUniversalTime()
            : DateTime.UtcNow.ToUniversalTime();
        OpenDays = (DateTimeEnd - DateTimeStart).TotalDays;
    }

    public override string ToString()
    {
        return $"{State} issue #{Number} \"{Title}\" by @{User} (open for {OpenDays:N2} days)";
    }
}
