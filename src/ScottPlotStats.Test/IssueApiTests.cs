namespace ScottPlotStats.Test;

[TestClass]
public class IssueApiTests
{
    [TestMethod]
    public void Test_LookupToken()
    {
        _ = new GitHubDataFetcher("scottplot", "scottplot");
    }

    [TestMethod]
    public void Test_FetchIssuesFromGitHub()
    {
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubIssueCollection issues = api.GetIssues(3);
        issues.Count.Should().BeGreaterThan(0);
        Console.WriteLine($"Finished loading {issues.Count:N0} issues.");
    }
}
