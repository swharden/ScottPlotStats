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
}