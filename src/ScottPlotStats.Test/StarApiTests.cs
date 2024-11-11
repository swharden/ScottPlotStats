namespace ScottPlotStats.Test;

[TestClass]
public class StarApiTests
{
    [TestMethod]
    public void Test_Star_Api()
    {
        bool updateDevFile = false;
        GitHubDataFetcher api = new("scottplot", "scottplot");
        GitHubStarsCollection stars1 = api.GetStars(updateDevFile ? 9999 : 3);
        stars1.Count.Should().BeGreaterThan(123);
        if (updateDevFile)
        {
            string saveAs = Path.GetFullPath(SampleData.StarsJsonFilePath);
            File.WriteAllText(saveAs, stars1.ToJson());
            Console.WriteLine($"Saved: {saveAs}");
        }

        string json = stars1.ToJson();
        GitHubStarsCollection stars2 = GitHubStarsCollection.FromJson(json);
        stars2.Count.Should().Be(stars1.Count);
    }
}
