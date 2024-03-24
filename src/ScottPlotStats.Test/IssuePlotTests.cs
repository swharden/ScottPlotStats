namespace ScottPlotStats.Test;

[TestClass]
public class IssuePlotTests
{
    [TestMethod]
    public void Test_Plot_CountOverTime()
    {
        GitHubIssuePlotting plotter = new(SampleData.IssueCollection);
        plotter.NumberOfOpenIssues().SavePng("plot-open.png", 600, 400);
        plotter.NumberOfIssuesOpened().SavePng("plot-opened.png", 600, 400);
        plotter.MeanOpenIssueDuration().SavePng("plot-duration.png", 600, 400);
        plotter.DaysSinceIssueClosed().SavePng("plot-closed.png", 600, 400);
        plotter.PercentLongRunningIssues().SavePng("plot-long.png", 600, 400);
        Console.Write($"Saved images in: {Path.GetFullPath("./")}");
    }
}
