using NUnit.Framework;
using System;

namespace NuGetPPT.Tests
{
    public class Tests
    {
        [Test]
        public void Test_Parse_DownloadCount()
        {
            string json = SampleData.ReadSampleData("nuget-query-scottplot.json");
            int count = NuGetApi.ReadTotalDownloads(json);
            Assert.AreEqual(87115, count);
        }

        [Test]
        [Ignore("don't run HTTP tests")]
        public void Test_Request_Json()
        {
            string json = NuGetApi.RequestJson("scottplot");
            int count = NuGetApi.ReadTotalDownloads(json);
            Assert.Greater(count, 50_000);
        }
    }
}