namespace ScottPlotStats.Test;

[TestClass]
public class StarPlottingTests
{
    [TestMethod]
    public void Test_Star_Plotting()
    {
        GitHubStarPlotting plotter = new(SampleData.StarCollection);
        plotter.StarsOverTime().SavePng("star-count.png", 600, 400);
        plotter.NewStarsPerWeek().SavePng("star-new-week.png", 600, 400);
        plotter.NewStarsPerMonth().SavePng("star-new-month.png", 600, 400);
        Console.WriteLine($"Saved images in: {Path.GetFullPath("./")}");
    }
}
