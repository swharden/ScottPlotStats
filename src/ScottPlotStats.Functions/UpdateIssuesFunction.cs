using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ScottPlotStats.Functions;

public class UpdateIssuesFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateIssuesFunction>();

    [Function("UpdateIssuesFunction")]
    public void Run(
#if DEBUG
    [TimerTrigger("0 0 0 1 1 0", RunOnStartup = true)] TimerInfo myTimer
#else
    [TimerTrigger("0 0 0 * * *")] TimerInfo myTimer
#endif
        )
    {
        try
        {
            GitHubIssueCollection issues = GetIssues();
            MakePlots(issues);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "exception thrown while executing function");
            throw;
        }
    }

    private GitHubIssueCollection GetIssues()
    {
        Logger.LogInformation("Creating GitHub data fetcher with custom token...");
        GitHubDataFetcher api = new("scottplot", "scottplot");
        Logger.LogInformation("Fetching issue pages via GitHub API...");
        GitHubIssueCollection issues = api.GetIssues();
        Logger.LogInformation("Retrieved {count} issues.", issues.Count);
        return issues;
    }

    private void MakePlots(GitHubIssueCollection issues)
    {
        BlobStorage blob = new(Logger);
        GitHubIssuePlotting plotter = new(issues);
        blob.SavePng(plotter.NumberOfOpenIssues(), "issues-open.png", 600, 400);
        blob.SavePng(plotter.NumberOfIssuesOpened(), "issues-opened.png", 600, 400);
        blob.SavePng(plotter.MeanOpenIssueDuration(), "issues-duration.png", 600, 400);
        blob.SavePng(plotter.DaysSinceIssueClosed(), "issues-closed.png", 600, 400);
    }
}
