namespace ScottPlotStats.Test;

internal class SampleData
{
    public static readonly string IssuePageJson = File.ReadAllText("../../../../../dev/SampleData/issue-page-1.json");
    public static readonly string IssuesJson = File.ReadAllText("../../../../../dev/SampleData/issues.json");
    public static readonly string StarsJson = File.ReadAllText("../../../../../dev/SampleData/stars.json");

    public static GitHubIssueCollection IssueCollection = GitHubIssueCollection.FromJson(IssuesJson);

    public static CountDatabase CountDatabase = GetNuGetCountDatabase();

    public static GitHubStarsCollection StarCollection = GitHubStarsCollection.FromJson(StarsJson);

    private static CountDatabase GetNuGetCountDatabase()
    {
        string CSV_FILE = "../../../../../dev/SampleData/scottplot-2024-02-10.csv";
        string txt = File.ReadAllText(CSV_FILE);
        CountDatabase db = CountDatabase.FromCsv(txt);
        return db;
    }
}
