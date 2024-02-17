namespace ScottPlotStats.Test;

[TestClass]
public class StarApiTests
{
    [TestMethod]
    public void Test_Star_Api()
    {
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubStarsCollection stars1 = api.GetStars(3);
        stars1.Count.Should().BeGreaterThan(123);

        string json = stars1.ToJson();
        GitHubStarsCollection stars2 = GitHubStarsCollection.FromJson(json);
        stars2.Count.Should().Be(stars1.Count);
    }
}
