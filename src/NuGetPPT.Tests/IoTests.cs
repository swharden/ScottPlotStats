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

            foreach (string ts in downloads.Keys)
                Console.WriteLine($"{ts}\t{downloads[ts]}");
        }

        [Test]
        public void Test_Logs_AreConsecutive()
        {
            string json = SampleData.ReadSampleData("logs-scottplot.json");
            var downloads = IO.FromJson(json);

            var last = new DateTime();
            foreach (string ts in downloads.Keys)
            {
                DateTime dt = DateTime.Parse(ts);
                Assert.Greater(dt, last);
                last = dt;
            }
        }

        [Test]
        public void Test_Logs_EncodeIdentically()
        {
            string jsonInput = SampleData.ReadSampleData("logs-scottplot.json");

            Dictionary<string, int> records = IO.FromJson(jsonInput);
            string jsonOutput = IO.ToJson(records, "scottplot");

            Assert.AreEqual(jsonInput, jsonOutput);
        }
    }
}
