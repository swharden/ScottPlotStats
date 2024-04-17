namespace ScottPlotStats.Test;

[TestClass]
public class StarPlottingTests
{
    [TestMethod]
    public void Test_Star_Plotting()
    {
        string saveFolder = Path.GetFullPath("./test-images");
        Directory.CreateDirectory(saveFolder);

        GitHubStarPlotting plotter = new(SampleData.StarCollection);
        plotter.StarsOverTime().SavePng(Path.Join(saveFolder, "star-count.png"), 600, 400);
        plotter.NewStarsPerWeek().SavePng(Path.Join(saveFolder, "star-new-week.png"), 600, 400);
        plotter.NewStarsPerMonth().SavePng(Path.Join(saveFolder, "star-new-month.png"), 600, 400);
        Console.WriteLine($"Saved images in: {saveFolder}");
    }
}
