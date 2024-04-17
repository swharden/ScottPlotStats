namespace ScottPlotStats.Test;

[TestClass]
public class NuGetPlottingTests
{
    [TestMethod]
    public void Test_Plot_CountsOverTime()
    {
        string saveFolder = Path.GetFullPath("./test-images");
        Directory.CreateDirectory(saveFolder);

        CountDatabase db = SampleData.CountDatabase;
        NuGetPlotting.DownloadCount(db).SavePng(Path.Join(saveFolder, "DownloadCount.png"), 600, 350);
        NuGetPlotting.DownloadRate(db).SavePng(Path.Join(saveFolder, "DownloadRate.png"), 600, 350);
    }
}