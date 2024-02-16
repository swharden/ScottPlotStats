namespace ScottPlotStats.Test;

[TestClass]
public class IssueJsonTests
{
    [TestMethod]
    public void Test_Issues_ToJson()
    {
        GitHubIssueCollection issues1 = SampleData.IssueCollection;
        string json = issues1.ToJson();
        json.Length.Should().BeGreaterThan(1000);

        GitHubIssueCollection issues2 = GitHubIssueCollection.FromJson(json);
        issues2.Count.Should().Be(issues1.Count);
    }
}
