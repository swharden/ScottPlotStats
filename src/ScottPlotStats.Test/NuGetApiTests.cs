namespace ScottPlotStats.Test;

[TestClass]
public class NuGetApiTests
{
    [TestMethod]
    public void Test_DownloadCount()
    {
        int count = NuGetAPI.GetDownloadCount("ScottPlot");
        count.Should().BeGreaterThan(900_000);
    }
}