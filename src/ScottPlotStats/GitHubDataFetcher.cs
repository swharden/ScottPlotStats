namespace ScottPlotStats;

public class GitHubDataFetcher
{
    public readonly string User;
    public readonly string Repo;
    public string BaseUrl => $"https://api.github.com/repos/{User}/{Repo}";

    private readonly HttpClient HttpIssueClient;

    private readonly HttpClient HttpStarClient;

    public GitHubDataFetcher(string user, string repo)
    {
        User = user;
        Repo = repo;

        string token = ReadGitHubToken();

        HttpIssueClient = new();
        HttpIssueClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        HttpIssueClient.DefaultRequestHeaders.Add("User-Agent", "Other");
        HttpIssueClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        HttpStarClient = new();
        HttpStarClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.star+json");
        HttpStarClient.DefaultRequestHeaders.Add("User-Agent", "Other");
        HttpStarClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    }

    private static string ReadGitHubToken()
    {
        string envFilePath = "../../../../../github-token.env";
        if (File.Exists(envFilePath))
        {
            string token = File.ReadAllText(envFilePath);
            Console.WriteLine($"Loaded {token.Length} byteGitHub token from local file");
            return token;
        }

        string? gitHubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN", EnvironmentVariableTarget.Process);
        if (gitHubToken is not null)
        {
            Console.WriteLine($"Loaded {gitHubToken.Length} byte GitHub token from environment variable");
            return gitHubToken;
        }

        throw new InvalidOperationException("GitHub token could not be located");
    }

    public GitHubIssueCollection GetIssues(int maximumPage = int.MaxValue)
    {
        GitHubIssueCollection issues = new();
        int lastPage = AddIssuesFromPage(issues, 1);
        for (int i = 2; i <= lastPage; i++)
        {
            AddIssuesFromPage(issues, i);
            if (i >= maximumPage)
                break;
        }
        return issues;
    }

    public GitHubStarsCollection GetStars(int maximumPage = int.MaxValue)
    {
        GitHubStarsCollection stars = new();

        int lastPage = AddStarsFromPage(stars, 1);
        for (int i = 2; i <= lastPage; i++)
        {
            AddStarsFromPage(stars, i);
            if (i >= maximumPage)
                break;
        }

        return stars;
    }

    private (string body, int lastPage, int remainingRequests) GetPage(HttpClient client, string url)
    {
        using HttpResponseMessage response = client.GetAsync(url).Result;
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Response error code {response.StatusCode}");

        Dictionary<string, IEnumerable<string>> headers = response.Headers.ToDictionary();
        int remainingRequests = GetRemainingRequests(headers);
        int lastPage = GetLastPageNumber(headers);
        string body = response.Content.ReadAsStringAsync().Result;
        return (body, lastPage, remainingRequests);
    }

    private int AddIssuesFromPage(GitHubIssueCollection issues, int page)
    {
        string url = $"{BaseUrl}/issues?state=all&per_page=100&page={page}";
        (string body, int lastPage, int remainingRequests) = GetPage(HttpIssueClient, url);

        issues.AddRangeFromJson(body);
        Console.WriteLine($"Processed page {page} of {lastPage} " +
            $"({body.Length / 1000:N2} kB) " +
            $"containing {issues.Count} issues ({remainingRequests:N0} requests remaining)");

        return lastPage;
    }

    private int AddStarsFromPage(GitHubStarsCollection stars, int page)
    {
        string url = $"{BaseUrl}/stargazers?per_page=100&page={page}";
        (string body, int lastPage, int remainingRequests) = GetPage(HttpStarClient, url);

        stars.AddRangeFromJson(body);
        Console.WriteLine($"Processed page {page} of {lastPage} " +
            $"({body.Length / 1000:N2} kB) " +
            $"containing {stars.Count} issues ({remainingRequests:N0} requests remaining)");

        return lastPage;
    }

    private static int GetLastPageNumber(Dictionary<string, IEnumerable<string>> headers)
    {
        string[] links = headers["Link"].Single().Split(",");
        var lastLinks = links.Where(x => x.Contains("rel=\"last\""));
        if (!lastLinks.Any())
            return -1;

        string lastLink = lastLinks.Single();
        int lastPage = int.Parse(lastLink.Split("&page=")[1].Split(">")[0]);
        return lastPage;
    }

    private int GetRemainingRequests(Dictionary<string, IEnumerable<string>> headers, int minimumRemaining = 100)
    {
        int remaining = int.Parse(headers["X-RateLimit-Remaining"].Single());
        if (remaining < minimumRemaining)
            throw new InvalidOperationException("Aborting due to low remaining requests");
        return remaining;
    }
}
