using ScottPlot;

namespace ScottPlotStats;

public static class NuGetPlotting
{
    public static ScottPlot.Plot DownloadCount(CountDatabase db)
    {
        List<DateTime> dates = [];
        List<int> counts = [];
        foreach (var record in db.GetRecords())
        {
            if (counts.Count == 0 || record.Count > counts.Last())
            {
                dates.Add(record.Date);
                counts.Add(record.Count);
            }
        }

        ScottPlot.Plot plot = new();

        var sp = plot.Add.ScatterLine(dates, counts);
        sp.LineWidth = 1.5f;
        sp.FillY = true;

        var txt = plot.Add.Text($"{counts.Last():N0}", dates.Last().ToOADate(), counts.Last());
        txt.LabelAlignment = ScottPlot.Alignment.LowerRight;
        txt.LabelFontSize = 22;
        txt.LabelBold = true;
        txt.LabelRotation = -60;
        txt.LabelOffsetY = 10;
        txt.LabelOffsetX = -20;

        plot.Title("ScottPlot NuGet Package Downloads");
        plot.Axes.Top.MaximumSize = 10;
        plot.Axes.DateTimeTicksBottom();

        plot.Add.HorizontalLine(0, 1, Colors.Black, LinePattern.DenselyDashed);

        return plot;
    }

    public static ScottPlot.Plot DownloadRate(CountDatabase db)
    {
        // create time bins
        TimeSpan binSize = TimeSpan.FromDays(7);
        DateTime firstBin = new(2019, 1, 1);
        DateTime lastBin = DateTime.Now;
        int binCount = (lastBin - firstBin).Days / binSize.Days - 2;
        DateTime[] binTimes = Enumerable
            .Range(0, binCount)
            .Select(x => firstBin + binSize * x)
            .ToArray();

        // get maximum download count for each bin
        int[] binCounts = new int[binTimes.Length];
        foreach (var record in db.GetRecords())
        {
            int index = (record.Date - firstBin).Days / binSize.Days;

            if (index >= binCounts.Length)
                continue;

            if (record.Count > binCounts[index])
                binCounts[index] = record.Count;
        }

        // correct for binTimes that have missing data
        for (int i = 1; i < binCounts.Length; i++)
        {
            if (binCounts[i] < binCounts[i - 1])
            {
                binCounts[i] = binCounts[i - 1];
            }
        }

        // calculate acceleration by week
        double[] binCountDeltas = new double[binTimes.Length];
        for (int i = 1; i < binCounts.Length; i++)
        {
            binCountDeltas[i] = binCounts[i] - binCounts[i - 1];
        }

        ScottPlot.Plot plot = new();

        var sp1 = plot.Add.Scatter(binTimes, binCountDeltas);
        sp1.MarkerSize = 0;
        sp1.LineWidth = 1.5f;
        sp1.Color = sp1.Color.WithAlpha(.5);
        sp1.FillY = true;
        sp1.LegendText = "Downloads per Week";

        double[] sma = ScottPlot.Statistics.Series.MovingAverage(binCountDeltas, window: 12, preserveLength: true);
        sma = sma.Select(x => double.IsNaN(x) ? 0 : x).ToArray();
        var sp2 = plot.Add.Scatter(binTimes, sma);
        sp2.MarkerSize = 0;
        sp2.LineWidth = 1.5f;
        sp2.Color = Colors.Black;
        sp2.LegendText = "3 Month Average";

        plot.Add.HorizontalLine(0, 1, Colors.Black, LinePattern.DenselyDashed);

        plot.ShowLegend(ScottPlot.Alignment.UpperLeft);
        plot.Title("ScottPlot NuGet Package Downloads per Week");
        plot.Axes.Top.MaximumSize = 10;
        plot.Axes.DateTimeTicksBottom();

        return plot;
    }
}
