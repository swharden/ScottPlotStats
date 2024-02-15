namespace GitHubApiTest;

internal static class SampleData
{
    public static readonly string IssuePageJson = File.ReadAllText("../../../../SampleData/issue-page-1.json");
    public static GitHubIssueCollection IssueCollection = SampleIssueCollection();

    private static GitHubIssueCollection SampleIssueCollection()
    {
        GitHubIssueCollection issues = new();
        issues.AddRangeFromJson(IssuePageJson);
        return issues;
    }
}
