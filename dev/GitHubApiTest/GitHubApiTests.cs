namespace GitHubApiTest;

public class Tests
{
    [Test]
    public void Test_LookupToken()
    {
        GitHubDataFetcher api = new("scottplot", "scottplot");
    }

    [Test]
    public void Test_FetchIssuesFromGitHub()
    {
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubIssueCollection issues = api.GetIssues();
        Assert.That(issues, Is.Not.Empty);
        Console.WriteLine($"Finished loading {issues.Count:N0} issues.");
        File.WriteAllText("issues.json", issues.ToJson());
    }

    [Test]
    public void Test_ParseIssueJson()
    {
        string body = File.ReadAllText("../../../SampleData/issue-page-1.json");

        GitHubIssueCollection issues = new();
        issues.AddRangeFromJson(body);
        Assert.That(issues, Is.Not.Empty);

        foreach (var issue in issues.GetIssues())
        {
            Console.WriteLine(issue);
        }
    }

    [Test]
    public void Test_IssuesToJson()
    {
        string body = File.ReadAllText("../../../SampleData/issue-page-1.json");
        GitHubIssueCollection issues1 = new();
        issues1.AddRangeFromJson(body);
        string json = issues1.ToJson();
        Assert.That(json.Length, Is.GreaterThan(1000));

        GitHubIssueCollection issues2 = GitHubIssueCollection.FromJson(json);
        Assert.That(issues2.Count, Is.EqualTo(issues1.Count));
    }
}