namespace ScottPlotStats.Test;

[TestClass]
public class IssuePlotTests
{
    [TestMethod]
    public void Test_Plot_CountOverTime()
    {
        GitHubIssueCollection issues = SampleData.IssueCollection;
        issues.Count.Should().BeGreaterThan(0);

        List<DateTime> dates = [];
        List<double> openCount = [];
        List<double> totalOpened = [];
        List<double> meanOpenTime = [];
        List<double> daysSinceLastClose = [];

        int lastCloseDays = 0;
        int total = 0;
        foreach ((var day, var dayIssues) in issues.GetIssuesByDay())
        {
            dates.Add(day.ToDateTime(TimeOnly.MinValue));
            openCount.Add(dayIssues.Count);
            total += dayIssues.Where(x => DateOnly.FromDateTime(x.DateTimeStart) == day).Count();
            totalOpened.Add(total);

            if (dayIssues.Count != 0)
                meanOpenTime.Add(dayIssues.Select(x => x.OpenDays).Average());
            else
                meanOpenTime.Add(0);

            var issuesClosedToday = dayIssues.Where(x => DateOnly.FromDateTime(x.DateTimeEnd) == day);
            if (issuesClosedToday.Any())
                lastCloseDays = 0;
            else
                lastCloseDays += 1;
            daysSinceLastClose.Add(lastCloseDays);
        }

        {
            ScottPlot.Plot plot = new();
            plot.Title("Number of Open Issues");
            var sp = plot.Add.Scatter(dates, openCount);
            sp.LineStyle.Width = 2;
            sp.MarkerSize = 0;
            plot.Axes.DateTimeTicksBottom();
            plot.SavePng("issues-open.png", 600, 400);
        }

        {
            ScottPlot.Plot plot = new();
            plot.Title("Total Number of Issues Opened");
            var sp = plot.Add.Scatter(dates, totalOpened);
            sp.LineStyle.Width = 2;
            sp.MarkerSize = 0;
            plot.Axes.DateTimeTicksBottom();
            plot.SavePng("issues-total.png", 600, 400);
        }

        {
            ScottPlot.Plot plot = new();
            plot.Title("Mean Open Issue Duration");
            var sp = plot.Add.Scatter(dates, meanOpenTime);
            sp.LineStyle.Width = 2;
            sp.MarkerSize = 0;
            plot.Axes.DateTimeTicksBottom();
            plot.SavePng("issues-open-time.png", 600, 400);
        }

        {
            ScottPlot.Plot plot = new();
            plot.Title("Days Since Last Issue was Closed");
            var sp = plot.Add.Scatter(dates, daysSinceLastClose);
            //sp.LineStyle.Width = 2;
            sp.MarkerSize = 0;
            plot.Axes.DateTimeTicksBottom();
            plot.SavePng("issues-close-days.png", 600, 400);
        }
    }
}
