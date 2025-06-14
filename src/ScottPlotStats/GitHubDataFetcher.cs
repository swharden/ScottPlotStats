namespace ScottPlotStats;

public class GitHubDataFetcher
{
    public readonly string User;
    public readonly string Repo;
    public string BaseUrl => $"https://api.github.com/repos/{User}/{Repo}";

    private readonly HttpClient HttpIssueClient;

    private readonly HttpClient HttpStarClient;

    public
        GitHubDataFetcher(string user, string repo)
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
            Console.WriteLine($"Loaded GitHub token from local file");
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
        string? nextUrl = AddIssuesFromPage(issues);
        while (nextUrl is not null)
        {
            nextUrl = AddIssuesFromPage(issues, nextUrl);
        }
        return issues;
    }

    public GitHubStarsCollection GetStars()
    {
        GitHubStarsCollection stars = new();

        string? nextUrl = AddStarsFromPage(stars);
        while (nextUrl is not null)
        {
            nextUrl = AddStarsFromPage(stars, nextUrl);
        }

        return stars;
    }

    private (string body, string? nextUrl, int thisPage, int remainingRequests) GetPage(HttpClient client, string url)
    {
        string thisPage = url.Split("&").Where(x => x.StartsWith("page=")).First().Split("=")[1];
        int thisPageNumber = int.Parse(thisPage);
        using HttpResponseMessage response = client.GetAsync(url).Result;
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Response error code {response.StatusCode}");

        Dictionary<string, IEnumerable<string>> headers = response.Headers.ToDictionary();
        int remainingRequests = GetRemainingRequests(headers);
        string? nextUrl = GetNextPageUrl(headers);
        string body = response.Content.ReadAsStringAsync().Result;
        return (body, nextUrl, thisPageNumber, remainingRequests);
    }

    private string? AddIssuesFromPage(GitHubIssueCollection issues, string? url = null)
    {
        url ??= $"{BaseUrl}/issues?state=all&per_page=100&page=1";
        var (body, nextUrl, thisPage, remainingRequests) = GetPage(HttpIssueClient, url);

        issues.AddRangeFromJson(body);
        Console.WriteLine($"Processed Issues page {thisPage} ({remainingRequests:N0} requests remaining)");

        return nextUrl;
    }

    private string? AddStarsFromPage(GitHubStarsCollection stars, string? url = null)
    {
        url ??= $"{BaseUrl}/stargazers?per_page=100&page=1";
        var (body, nextUrl, thisPage, remainingRequests) = GetPage(HttpStarClient, url);

        stars.AddRangeFromJson(body);
        Console.WriteLine($"Processed Stargazers page {thisPage} ({remainingRequests:N0} requests remaining)");

        return nextUrl;
    }

    private static string? GetNextPageUrl(Dictionary<string, IEnumerable<string>> headers)
    {
        if (!headers["Link"].Any())
            return null;

        string? line = headers["Link"].Last().Split(",").Where(x => x.Contains("rel=\"next\"")).FirstOrDefault();
        if (line is null)
            return null;

        return line.Split("<")[1].Split(">")[0];
    }

    private int GetRemainingRequests(Dictionary<string, IEnumerable<string>> headers, int minimumRemaining = 100)
    {
        int remaining = int.Parse(headers["X-RateLimit-Remaining"].Single());
        if (remaining < minimumRemaining)
            throw new InvalidOperationException("Aborting due to low remaining requests");
        return remaining;
    }
}
