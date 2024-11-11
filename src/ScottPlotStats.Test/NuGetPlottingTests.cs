namespace ScottPlotStats.Test;

[TestClass]
public class NuGetPlottingTests
{
    [TestMethod]
    public void Test_Plotting_NuGetDownloads()
    {
        CountDatabase db = SampleData.CountDatabase;

        HtmlReport report = new();
        report.AddEmbeddedSvg(NuGetPlotting.DownloadCount(db));
        report.AddEmbeddedSvg(NuGetPlotting.DownloadRate(db));
        report.SaveAs("report2.html");
    }

    [TestMethod]
    public void Test_Plotting_GitHubStars()
    {
        GitHubStarPlotting plotter = new(SampleData.StarCollection);

        HtmlReport report = new();
        report.AddEmbeddedSvg(plotter.StarsOverTime());
        report.AddEmbeddedSvg(plotter.NewStarsPerWeek());
        report.AddEmbeddedSvg(plotter.NewStarsPerMonth());
        report.SaveAs("report3.html");
    }
}