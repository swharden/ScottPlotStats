using ScottPlot;
using ScottPlot.Colormaps;
using SkiaSharp;

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

    /*
    private static Plot StyleFilledLineGraph(IEnumerable<DateTime> dates, IEnumerable<int> values, string title, bool average = true)
    {
        return StyleFilledLineGraph(dates, values.Select(x => (double)x), title, average);
    }
    */

    private static Plot StyleFilledLineGraph(IEnumerable<DateTime> dates, IEnumerable<double> values, string title, bool average = true)
    {
        DateTime[] dates2 = dates.ToArray();
        double[] values2 = values.ToArray();

        Plot plot = new();
        plot.Title(title);
        plot.Axes.DateTimeTicksBottom();
        plot.Add.HorizontalLine(0, 1, Colors.Black, LinePattern.DenselyDashed);
        var sp = plot.Add.ScatterLine(dates2, values2);
        sp.LineStyle.Width = 1.5f;
        sp.LineColor = average ? Colors.C0.WithAlpha(.5) : Colors.C0;
        sp.FillY = true;
        sp.FillYColor = Colors.C0.WithAlpha(.2);

        if (average)
        {
            double[] runningAverage = new double[values2.Length];
            for (int i = 0; i < values2.Length; i++)
            {
                DateTime endDate = dates2[i];
                DateTime startDate = dates2[i] - TimeSpan.FromDays(90);

                int count = 0;
                double sum = 0;
                for (int j = 0; j < values2.Length; j++)
                {
                    if (dates2[j] >= startDate && dates2[j] <= endDate)
                    {
                        count += 1;
                        sum += values2[j];
                    }
                }
                runningAverage[i] = sum / count;
            }

            var sp2 = plot.Add.ScatterLine(dates2, runningAverage);
            sp2.LineStyle.Width = 1.5f;
            sp2.LineColor = Colors.Black;
            sp2.LegendText = "3 month average";

            sp.LegendText = "Daily Value";

            plot.Legend.Alignment = Alignment.UpperRight;
        }

        return plot;
    }

    public ScottPlot.Plot NumberOfOpenIssues() => StyleFilledLineGraph(Dates, OpenCount, "Number of Open Issues", false);
    public ScottPlot.Plot NumberOfIssuesOpened() => StyleFilledLineGraph(Dates, TotalOpened, "Total Number of Issues Opened", false);
    public ScottPlot.Plot MeanOpenIssueDuration() => StyleFilledLineGraph(Dates, MeanOpenTime, "Mean Open Issue Duration");
    public ScottPlot.Plot DaysSinceIssueClosed() => StyleFilledLineGraph(Dates, DaysSinceLastClose, "Days Since an Issue was Closed");
    public ScottPlot.Plot PercentLongRunningIssues() => StyleFilledLineGraph(Dates, PercentLongRunning, "% Open Over 90 Days");

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
            bar.LineWidth = 0;
            bar.Size = 10; //oversize to avoid anti-alias errors
        }
        bars.LegendText = "Issues per week";
        bars.Color = Colors.C0.Lighten(.5);

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
