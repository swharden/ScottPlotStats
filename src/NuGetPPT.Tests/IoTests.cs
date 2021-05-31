using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NuGetPPT.Tests
{
    class IoTests
    {
        [Test]
        public void Test_Logs_ReadCorrectly()
        {
            string json = SampleData.ReadSampleData("logs-scottplot.json");
            var downloads = IO.FromJson(json);
            Assert.AreEqual(343, downloads.Count);

            foreach (var d in downloads)
                Console.WriteLine($"{d.Timestamp}\t{d.Downloads}");
        }

        [Test]
        public void Test_Logs_AreConsecutiveAfterSorting()
        {
            string json = SampleData.ReadSampleData("logs-scottplot.json");
            var downloads = IO.FromJson(json);

            downloads.Sort();

            var last = new DateTime();
            foreach (var d in downloads)
            {
                Assert.Greater(d.DateTime, last);
                last = d.DateTime;
            }
        }

        [Test]
        public void Test_Logs_EncodeIdentically()
        {
            string jsonInput = SampleData.ReadSampleData("logs-scottplot.json");

            var records = IO.FromJson(jsonInput);
            string jsonOutput = IO.ToJson(records, "scottplot");

            Assert.AreEqual(jsonInput, jsonOutput);
        }
    }
}
