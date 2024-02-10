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

    [TestMethod]
    public void Test_Database_Load()
    {
        string CSV_FILE = "../../../../../dev/SampleData/scottplot-2024-02-10.csv";
        string txt = File.ReadAllText(CSV_FILE);
        var records = Database.GetRecords(txt);
        records.Should().NotBeNullOrEmpty();
        records.Count.Should().BeGreaterThan(1900);
        records.Last().Count.Should().BeGreaterThan(900_000);
        Console.WriteLine(records.Last());
    }
}