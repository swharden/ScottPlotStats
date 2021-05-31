using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NuGetPPT
{
    public static class Plotting
    {
        public static System.Drawing.Bitmap MakePlot(string packageName, List<DownloadRecord> records)
        {
            double[] xs = records.Select(x => x.DateTime.ToOADate()).ToArray();
            double[] ys = records.Select(x => (double)x.Downloads).ToArray();

            var plt = new ScottPlot.Plot(600, 350);
            var sp = plt.AddScatterLines(xs, ys, lineWidth: 2);
            plt.Title($"NuGet Download Statistics for {packageName}");
            plt.XAxis.DateTimeFormat(true);
            plt.YLabel("Total Downloads");

            string day = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string time = DateTime.UtcNow.ToString("H:mm");
            var dateAnnotation = plt.AddAnnotation($"updated {day} at {time} UTC", -3, -1);
            dateAnnotation.Border = false;
            dateAnnotation.Shadow = false;
            dateAnnotation.Background = false;
            dateAnnotation.Font.Color = System.Drawing.Color.FromArgb(150, System.Drawing.Color.Black);
            dateAnnotation.Font.Name = ScottPlot.Drawing.InstalledFont.Monospace();
            dateAnnotation.Font.Size = 10;

            return plt.Render();
        }

        public static byte[] GetPngBytes(System.Drawing.Bitmap bmp)
        {
            using MemoryStream stream = new();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
    }
}
