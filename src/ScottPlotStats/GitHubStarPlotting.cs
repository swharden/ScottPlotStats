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
        sp.FillY = true;
        plt.Axes.DateTimeTicksBottom();
        plt.Title($"ScottPlot GitHub Stars");

        int stargazersToShow = 13;
        string[] stargazers = Stars.GetStargazerNames();
        string[] lastNames = stargazers.Skip(stargazers.Length - stargazersToShow).Select(x => "  " + x.Trim()).ToArray();
        string text = "Recent Stargazers:\n" + string.Join("\n", lastNames);

        plt.Add.HorizontalLine(0, 1, Colors.Black, LinePattern.DenselyDashed);

        var an = plt.Add.Annotation(text, ScottPlot.Alignment.UpperLeft);
        an.LabelBackgroundColor = Color.FromHex("#f6f6dd");
        an.LabelShadowColor = Colors.Black.WithAlpha(.2);
        an.LabelBorderColor = Colors.Black;
        an.LabelBorderWidth = 1;

        var txt = plt.Add.Text($"{counts.Last():N0}", dates.Last().ToOADate(), counts.Last());
        txt.LabelAlignment = ScottPlot.Alignment.LowerRight;
        txt.LabelFontSize = 22;
        txt.LabelBold = true;
        txt.LabelRotation = -45;
        txt.LabelOffsetY = 10;
        txt.LabelOffsetX = -20;

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

        var sp = plt.Add.ScatterLine(bins, counts);
        sp.LineWidth = 2;
        sp.FillY = true;

        plt.Add.HorizontalLine(0, 1, Colors.Black, LinePattern.DenselyDashed);

        plt.Axes.DateTimeTicksBottom();
        plt.Title($"ScottPlot New Stars per {name}");
        return plt;
    }
}
