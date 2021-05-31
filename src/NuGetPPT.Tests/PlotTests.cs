using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuGetPPT.Tests
{
    class PlotTests
    {
        [Test]
        public void Test_Plot_Creation()
        {
            string json = SampleData.ReadSampleData("logs-scottplot.json");
            var downloads = IO.FromJson(json);
            var bmp = NuGetPPT.Plotting.MakePlot("ScottPlot", downloads);
            byte[] bytes = NuGetPPT.Plotting.GetPngBytes(bmp);
            System.IO.File.WriteAllBytes("bytes.png", bytes);
        }
    }
}
