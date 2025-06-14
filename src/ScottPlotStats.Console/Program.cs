using ScottPlotStats;

string issuesJsonFilePath = Path.GetFullPath("../../../../../dev/SampleData/issues.json");
if (!File.Exists(issuesJsonFilePath)) throw new FileNotFoundException(issuesJsonFilePath);

string starsJsonFilePath = Path.GetFullPath("../../../../../dev/SampleData/stars.json");
if (!File.Exists(starsJsonFilePath)) throw new FileNotFoundException(starsJsonFilePath);

GitHubDataFetcher fetcher = new("scottplot", "scottplot");
File.WriteAllText(issuesJsonFilePath, fetcher.GetIssues().ToJson());
File.WriteAllText(starsJsonFilePath, fetcher.GetStars().ToJson());