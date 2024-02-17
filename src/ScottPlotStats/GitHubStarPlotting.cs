namespace ScottPlotStats;

public class GitHubStarPlotting(GitHubStarsCollection stars)
{
    readonly GitHubStarsCollection Stars = stars;

    public ScottPlot.Plot StarsOverTime()
    {
        DateTime[] dates = Stars.GetStarDates();
        int[] counts = Enumerable.Range(1, dates.Length).ToArray();

        ScottPlot.Plot plt = new();
        var sp = plt.Add.Scatter(dates, counts);
        sp.MarkerSize = 0;
        sp.LineWidth = 2;
        plt.Axes.DateTimeTicksBottom();
        plt.Title("ScottPlot Stars on GitHub");
        return plt;
    }

    public ScottPlot.Plot NewStarsPerWeek()
    {
        return NewStarsPer(TimeSpan.FromDays(7), "Week");
    }

    public ScottPlot.Plot NewStarsPerMonth()
    {
        return NewStarsPer(TimeSpan.FromDays(30), "Month");
    }

    private ScottPlot.Plot NewStarsPer(TimeSpan span, string name)
    {
        ScottPlot.Plot plt = new();

        DateTime[] starDates = Stars.GetStarDates();
        DateTime start = starDates.First();
        DateTime last = starDates.Last();
        int binCount = (int)((last - start).TotalDays / span.TotalDays) + 1;

        DateTime[] bins = Enumerable.Range(0, binCount).Select(x => start + TimeSpan.FromDays(7 * x)).ToArray();
        int[] counts = new int[bins.Length];

        foreach (DateTime dt in starDates)
        {
            int bin = (int)((dt - start).TotalDays / span.TotalDays);
            counts[bin] += 1;
        }

        var sp = plt.Add.Scatter(bins, counts);
        sp.MarkerSize = 0;
        sp.LineWidth = 2;
        plt.Axes.DateTimeTicksBottom();
        plt.Title($"ScottPlot New Stars per {name}");
        return plt;
    }
}
