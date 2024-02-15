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
        GitHubIssueCollection issues = api.GetIssues(3);
        Assert.That(issues, Is.Not.Empty);
        Console.WriteLine($"Finished loading {issues.Count:N0} issues.");
    }

    [Test]
    public void Test_ParseIssueJson()
    {
        GitHubIssueCollection issues = new();
        issues.AddRangeFromJson(SampleData.IssuePageJson);
        Assert.That(issues, Is.Not.Empty);

        foreach (var issue in issues.GetIssues())
        {
            Console.WriteLine(issue);
        }
    }

    [Test]
    public void Test_IssuesToJson()
    {
        GitHubIssueCollection issues1 = new();
        issues1.AddRangeFromJson(SampleData.IssuePageJson);
        string json = issues1.ToJson();
        Assert.That(json.Length, Is.GreaterThan(1000));

        GitHubIssueCollection issues2 = GitHubIssueCollection.FromJson(json);
        Assert.That(issues2.Count, Is.EqualTo(issues1.Count));
    }
}