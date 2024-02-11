namespace ScottPlotStats.Test;

[TestClass]
public class PlottingTests
{
    [TestMethod]
    public void Test_Plot_CountsOverTime()
    {
        string CSV_FILE = "../../../../../dev/SampleData/scottplot-2024-02-10.csv";
        string txt = File.ReadAllText(CSV_FILE);
        CountDatabase db = CountDatabase.FromCsv(txt);

        byte[] bytes1 = Plot.DownloadCount(db, 600, 350);
        File.WriteAllBytes("DownloadCount.png", bytes1);

        byte[] bytes2 = Plot.DownloadRate(db, 600, 350);
        File.WriteAllBytes("DownloadRate.png", bytes2);
    }
}