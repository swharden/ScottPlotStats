using System.Runtime.Versioning;

namespace ScottPlotStats;

public static class Plot
{
    public static byte[] DownloadCount(CountDatabase db, int width, int height)
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
        var sp = plot.Add.Scatter(dates, counts);
        sp.MarkerSize = 0;
        sp.LineWidth = 2;
        plot.Title("ScottPlot Nuget Package Total Downloads");
        plot.Axes.Top.MaximumSize = 10;
        plot.Axes.DateTimeTicksBottom();

        return plot.GetImageBytes(width, height, ScottPlot.ImageFormat.Png);
    }

    public static byte[] DownloadRate(CountDatabase db, int width, int height)
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
        int[] binCountDeltas = new int[binTimes.Length];
        for (int i = 1; i < binCounts.Length; i++)
        {
            binCountDeltas[i] = binCounts[i] - binCounts[i - 1];
        }

        ScottPlot.Plot plot = new();
        var sp = plot.Add.Scatter(binTimes, binCountDeltas);
        sp.MarkerSize = 0;
        sp.LineWidth = 2;
        plot.Title("ScottPlot Nuget Package Downloads per Week");
        plot.Axes.Top.MaximumSize = 10;
        plot.Axes.DateTimeTicksBottom();

        return plot.GetImageBytes(width, height, ScottPlot.ImageFormat.Png);
    }
}
