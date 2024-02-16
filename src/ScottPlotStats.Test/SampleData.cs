namespace ScottPlotStats.Test;

internal class SampleData
{
    public static readonly string IssuePageJson = File.ReadAllText("../../../../../dev/SampleData/issue-page-1.json");
    public static readonly string IssuesJson = File.ReadAllText("../../../../../dev/SampleData/issues.json");

    public static GitHubIssueCollection IssueCollection = GitHubIssueCollection.FromJson(IssuesJson);
}
