using ScottPlot;
using ScottPlot.Colormaps;

namespace ScottPlotStats;

public class GitHubIssuePlotting
{
    readonly DateOnly[] OpenDates;
    readonly List<DateTime> Dates = [];
    readonly List<double> OpenCount = [];
    readonly List<double> TotalOpened = [];
    readonly List<double> MeanOpenTime = [];
    readonly List<double> MedianOpenTime = [];
    readonly List<double> DaysSinceLastClose = [];
    readonly List<double> PercentLongRunning = [];

    public GitHubIssuePlotting(GitHubIssueCollection issues)
    {
        OpenDates = issues.GetIssues().Select(x => DateOnly.FromDateTime(x.DateTimeStart)).ToArray();
        int lastCloseDays = 0;
        int total = 0;
        foreach ((var day, var dayIssues) in issues.GetIssuesByDay())
        {
            Dates.Add(day.ToDateTime(TimeOnly.MinValue));
            OpenCount.Add(dayIssues.Count);
            total += dayIssues.Where(x => DateOnly.FromDateTime(x.DateTimeStart) == day).Count();
            TotalOpened.Add(total);

            if (dayIssues.Count != 0)
            {
                double[] openDays = dayIssues.Select(x => x.OpenDays).ToArray();
                Array.Sort(openDays);

                MeanOpenTime.Add(openDays.Average());

                double medianOpenDays = openDays[openDays.Length / 2];
                MedianOpenTime.Add(medianOpenDays);

                int issuesOverMonth = openDays.Where(x => x > 90).Count();
                PercentLongRunning.Add(100.0 * issuesOverMonth / openDays.Length);
            }
            else
            {
                MeanOpenTime.Add(0);
                MedianOpenTime.Add(0);
                PercentLongRunning.Add(0);
            }

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

    public ScottPlot.Plot PercentLongRunningIssues()
    {
        ScottPlot.Plot plot = new();
        plot.Title("Long-Running Issues");

        var sp2 = plot.Add.Scatter(Dates, PercentLongRunning);
        sp2.MarkerSize = 0;

        plot.YLabel("% Open Over 90 Days");
        plot.Axes.DateTimeTicksBottom();
        return plot;
    }

    private static double[] FilterForward(double[] values, int windowSize)
    {
        double[] smooth = new double[values.Length];

        double runningSum = 0;

        for (int i = 0; i < smooth.Length; i++)
        {
            runningSum += values[i];

            if (i >= windowSize)
            {
                runningSum -= values[i - windowSize];
                smooth[i] = runningSum / windowSize;
            }
            else
            {
                smooth[i] = runningSum / (i + 1);
            }
        }

        return smooth;
    }

    public ScottPlot.Plot NewIssuesPerWeek()
    {
        ScottPlot.Plot plot = new();

        int weekCount = Dates.Count / 7;
        DateTime[] weekStarts = Enumerable.Range(0, weekCount).Select(x => Dates[x * 7]).ToArray();
        double[] weekNewIssues = new double[weekCount];

        for (int weekIndex = 0; weekIndex < weekCount; weekIndex++)
        {
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                DateOnly day = DateOnly.FromDateTime(Dates[weekIndex * 7 + dayIndex]);
                weekNewIssues[weekIndex] += OpenDates.Count(x => x == day);
            }
        }

        plot.Title("New Issues per Week");

        var bars = plot.Add.Bars(weekStarts.Select(x => x.ToOADate()), weekNewIssues);
        foreach (var bar in bars.Bars)
        {
            bar.BorderLineWidth = 0;
            bar.Size = 10; //oversize to avoid anti-alias errors
        }
        bars.LegendText = "Issues per week";

        double[] smoothed = FilterForward(weekNewIssues, 12);
        var sp = plot.Add.ScatterLine(weekStarts, smoothed);
        sp.LineWidth = 2;
        sp.LineColor = Colors.Black;
        sp.LegendText = "3 month average";

        plot.Legend.Alignment = Alignment.UpperLeft;

        plot.YLabel("# New Issues");
        plot.Axes.DateTimeTicksBottom();
        plot.Axes.Margins(bottom: 0);
        return plot;
    }
}
