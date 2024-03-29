﻿namespace ScottPlotStats.Test;

[TestClass]
public class NuGetDatabaseTests
{
    [TestMethod]
    public void Test_Database_LoadFromCsv()
    {
        string CSV_FILE = "../../../../../dev/SampleData/scottplot-2024-02-10.csv";
        string txt = File.ReadAllText(CSV_FILE);
        CountDatabase db = CountDatabase.FromCsv(txt);
        Console.WriteLine(db);
        db.RecordCount.Should().BeGreaterThan(1900);
        db.HighestCount.Should().BeGreaterThan(900_000);
    }
}