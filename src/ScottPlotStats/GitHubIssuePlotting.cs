namespace ScottPlotStats;

public class GitHubIssuePlotting
{
    readonly List<DateTime> Dates = [];
    readonly List<double> OpenCount = [];
    readonly List<double> TotalOpened = [];
    readonly List<double> MeanOpenTime = [];
    readonly List<double> DaysSinceLastClose = [];

    public GitHubIssuePlotting(GitHubIssueCollection issues)
    {
        int lastCloseDays = 0;
        int total = 0;
        foreach ((var day, var dayIssues) in issues.GetIssuesByDay())
        {
            Dates.Add(day.ToDateTime(TimeOnly.MinValue));
            OpenCount.Add(dayIssues.Count);
            total += dayIssues.Where(x => DateOnly.FromDateTime(x.DateTimeStart) == day).Count();
            TotalOpened.Add(total);

            if (dayIssues.Count != 0)
                MeanOpenTime.Add(dayIssues.Select(x => x.OpenDays).Average());
            else
                MeanOpenTime.Add(0);

            var issuesClosedToday = dayIssues.Where(x => DateOnly.FromDateTime(x.DateTimeEnd) == day);
            if (issuesClosedToday.Any())
                lastCloseDays = 0;
            else
                lastCloseDays += 1;
            DaysSinceLastClose.Add(lastCloseDays);
        }
    }

    public ScottPlot.Plot NumberOfOpenIssues()
    {
        ScottPlot.Plot plot = new();
        plot.Title("Number of Open Issues");
        var sp = plot.Add.Scatter(Dates, OpenCount);
        sp.LineStyle.Width = 2;
        sp.MarkerSize = 0;
        plot.Axes.DateTimeTicksBottom();
        return plot;
    }

    public ScottPlot.Plot NumberOfIssuesOpened()
    {
        ScottPlot.Plot plot = new();
        plot.Title("Total Number of Issues Opened");
        var sp = plot.Add.Scatter(Dates, TotalOpened);
        sp.LineStyle.Width = 2;
        sp.MarkerSize = 0;
        plot.Axes.DateTimeTicksBottom();
        return plot;
    }

    public ScottPlot.Plot MeanOpenIssueDuration()
    {
        ScottPlot.Plot plot = new();
        plot.Title("Mean Open Issue Duration");
        var sp = plot.Add.Scatter(Dates, MeanOpenTime);
        sp.LineStyle.Width = 2;
        sp.MarkerSize = 0;
        plot.Axes.DateTimeTicksBottom();
        return plot;
    }

    public ScottPlot.Plot DaysSinceIssueClosed()
    {
        ScottPlot.Plot plot = new();
        plot.Title("Days Since an Issue was Closed");
        var sp = plot.Add.Scatter(Dates, DaysSinceLastClose);
        sp.MarkerSize = 0;
        plot.Axes.DateTimeTicksBottom();
        return plot;
    }
}
