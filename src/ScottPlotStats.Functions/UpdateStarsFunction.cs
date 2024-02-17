using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ScottPlotStats.Functions;

public class UpdateStarsFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateStarsFunction>();

    [Function("UpdateStarsFunction")]
    public void Run(
#if DEBUG
    [TimerTrigger("0 0 0 1 1 0", RunOnStartup = true)] TimerInfo myTimer
#else
    [TimerTrigger("0 0 * * * *")] TimerInfo myTimer
#endif
        )
    {
        Logger.LogInformation("Creating GitHub data fetcher with custom token...");
        GitHubDataFetcher api = new("scottplot", "scottplot");
        Logger.LogInformation("Fetching star pages via GitHub API...");
        GitHubStarsCollection stars = api.GetStars();
        Logger.LogInformation("Retrieved {count} stars.", stars.Count);

        BlobStorage blob = new(Logger);
        GitHubStarPlotting plotter = new(stars);
        blob.SavePng(plotter.StarsOverTime(), "stars.png", 600, 400);
        blob.SavePng(plotter.NewStarsPerWeek(), "stars-week.png", 600, 400);
        blob.SavePng(plotter.NewStarsPerMonth(), "stars-month.png", 600, 400);

        blob.SaveText(stars.GetLogJson(), "stars-log.txt");
    }
}
