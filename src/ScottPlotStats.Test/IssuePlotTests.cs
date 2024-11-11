namespace ScottPlotStats.Test;

[TestClass]
public class IssuePlotTests
{
    [TestMethod]
    public void Test_Plot_CountOverTime()
    {
        GitHubIssuePlotting plotter = new(SampleData.IssueCollection);

        HtmlReport report = new();
        report.AddEmbeddedSvg(plotter.NumberOfOpenIssues());
        report.AddEmbeddedSvg(plotter.NumberOfIssuesOpened());
        report.AddEmbeddedSvg(plotter.MeanOpenIssueDuration());
        report.AddEmbeddedSvg(plotter.DaysSinceIssueClosed());
        report.AddEmbeddedSvg(plotter.PercentLongRunningIssues());
        report.AddEmbeddedSvg(plotter.NewIssuesPerWeek());
        report.WriteAllText("report.html");
    }
}
