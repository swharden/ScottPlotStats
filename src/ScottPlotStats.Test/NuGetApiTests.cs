namespace ScottPlotStats.Test;

[TestClass]
public class NuGetApiTests
{
    [TestMethod]
    public void Test_NuGetAPI_GetCount()
    {
        var primaryRecord = NuGetAPI.GetPrimaryCount();
        primaryRecord.Count.Should().BeGreaterThan(900_000);
    }

    [TestMethod]
    public void Test_Database_LoadFromCsv()
    {
        string CSV_FILE = "../../../../../dev/SampleData/scottplot-2024-02-10.csv";
        string txt = File.ReadAllText(CSV_FILE);
        CountDatabase db = CountDatabase.FromCsv(txt);
        Console.WriteLine(db);
        db.Count.Should().BeGreaterThan(1900);
        db.HighestCount.Should().BeGreaterThan(900_000);
    }
}