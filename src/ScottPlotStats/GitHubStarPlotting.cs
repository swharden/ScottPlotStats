using ScottPlot;
using System.Text;

namespace ScottPlotStats;

public class GitHubStarPlotting(GitHubStarsCollection stars)
{
    readonly GitHubStarsCollection Stars = stars;

    public ScottPlot.Plot StarsOverTime()
    {
        DateTime[] dates = Stars.GetStarDates();
        int[] counts = Enumerable.Range(1, dates.Length).ToArray();

        ScottPlot.Plot plt = new();
        var sp = plt.Add.ScatterLine(dates, counts);
        sp.LineWidth = 2;
        plt.Axes.DateTimeTicksBottom();
        plt.Title($"ScottPlot GitHub Stars");

        /*
        var an1 = plt.Add.Annotation($"Updated {DateTime.Now.ToShortDateString()} at {DateTime.Now.ToShortTimeString()} EST", Alignment.LowerRight);
        an1.Label.BackgroundColor = Colors.White.MixedWith(Colors.Black, .05);
        an1.Label.ShadowColor = Colors.Transparent;
        an1.Label.BorderColor = Colors.White.MixedWith(Colors.Black, .1);
        an1.Label.ForeColor = Colors.Black;
        */

        StringBuilder sb = new();
        sb.AppendLine("Recent Stargazers:");
        int stargazersToShow = 20;
        string[] stargazers = Stars.GetStargazerNames();
        string[] lastNames = stargazers.Skip(stargazers.Length - stargazersToShow).ToArray();
        foreach (string name in lastNames)
        {
            sb.AppendLine(name);
        }

        var an = plt.Add.Annotation(sb.ToString().Trim(), ScottPlot.Alignment.UpperLeft);
        an.Label.BackgroundColor = Color.FromHex("#f6f6dd");
        an.Label.ShadowColor = Colors.Black.WithAlpha(.2);
        an.Label.BorderColor = Colors.Black;
        an.Label.BorderWidth = 1;
        an.Label.AntiAlias = true;

        var txt = plt.Add.Text($"{counts.Last():N0}", dates.Last().ToOADate(), counts.Last());
        txt.Label.Alignment = ScottPlot.Alignment.LowerRight;
        txt.Label.FontSize = 22;
        txt.Label.Bold = true;
        txt.Label.Rotation = -45;
        txt.Label.OffsetY = 10;
        txt.Label.OffsetX = -20;

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
