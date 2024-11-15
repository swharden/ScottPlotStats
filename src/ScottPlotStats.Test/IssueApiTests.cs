﻿namespace ScottPlotStats.Test;

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
        bool updateDevFile = false;
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubIssueCollection issues = api.GetIssues(updateDevFile ? 999 : 3);
        issues.Count.Should().BeGreaterThan(0);
        Console.WriteLine($"Finished loading {issues.Count:N0} issues.");

        if (updateDevFile)
        {
            string saveAs = Path.GetFullPath(SampleData.IssuesJsonFilePath);
            File.WriteAllText(saveAs, issues.ToJson());
            Console.WriteLine($"Wrote: {saveAs}");
        }
    }

    [TestMethod]
    public void Test_FetchStarsFromGitHub()
    {
        bool updateDevFile = false;
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubStarsCollection starCollection = api.GetStars(updateDevFile ? 999 : 3);
        starCollection.Count.Should().BeGreaterThan(0);
        Console.WriteLine($"Finished loading {starCollection.Count:N0} issues.");

        if (updateDevFile)
        {
            string saveAs = Path.GetFullPath(SampleData.StarsJsonFilePath);
            File.WriteAllText(saveAs, starCollection.ToJson());
            Console.WriteLine($"Wrote: {saveAs}");
        }
    }
}
