namespace ScottPlotStats.Test;

[TestClass]
public class NuGetPlottingTests
{
    [TestMethod]
    public void Test_Plot_CountsOverTime()
    {
        CountDatabase db = SampleData.CountDatabase;
        NuGetPlotting.DownloadCount(db).SavePng("DownloadCount.png", 600, 350);
        NuGetPlotting.DownloadRate(db).SavePng("DownloadRate.png", 600, 350);
    }
}