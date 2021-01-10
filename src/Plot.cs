using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Drawing;
using Microsoft.Azure.Cosmos.Table;
using Azure.Storage.Blobs;
using System.IO;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Diagnostics;

namespace PackagePopularityTracker.Functions
{
    public static class Plot
    {
        const string OncePerDay = "0 23 10 * * *"; // at 10:23 AM UTC (5:23 AM EST)

        [FunctionName("UpdatePackagePlots")]
        public static void UpdatePackagePlots([TimerTrigger(OncePerDay)] TimerInfo myTimer, ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);

            // get all stats
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable statsTable = tableClient.GetTableReference("packageStats");
            PackageStat[] allStats = statsTable.CreateQuery<PackageStat>().ToArray().OrderBy(x => x.Timestamp).ToArray();
            string[] packageNames = allStats.Select(x => x.Package).Distinct().ToArray();
            log.LogInformation($"Found {allStats.Length} total records.");
            log.LogInformation($"Identified {packageNames.Length} unique packages.");

            // create individual plots
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("$web");
            foreach (string packageName in packageNames)
            {
                // extract unique X/Y points for this package
                PackageStat[] stats = allStats.Where(x => x.Package == packageName).ToArray();
                SaveJsonStats(packageName, stats, containerClient);
                log.LogInformation($"Package {packageName} has {stats.Length} records.");
                double[] datetimes = stats.Select(x => x.Timestamp.DateTime.ToOADate()).ToArray();
                double[] downloads = stats.Select(x => (double)x.Downloads).ToArray();
                var (xs, ys) = UniquePoints(datetimes, downloads);

                // create the plot with ScottPlot
                var plt = new ScottPlot.Plot(600, 400);
                plt.Title(packageName);
                plt.YLabel("Downloads");
                plt.PlotStep(xs, ys, lineWidth: 2);
                plt.Ticks(dateTimeX: true);

                // Save the output as a Bitmap in blob storage
                string filePath = $"packagestats/{packageName}.png";
                BlobClient blobClient = containerClient.GetBlobClient(filePath);
                Bitmap bmp = plt.GetBitmap(true);
                using MemoryStream memoryStream = new MemoryStream();
                bmp.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                blobClient.Upload(memoryStream, overwrite: true);
            }
        }

        private static void SaveJsonStats(string packageName, PackageStat[] stats, BlobContainerClient containerClient)
        {
            var sb = new System.Text.StringBuilder("[");
            int lastCount = -1;
            foreach (PackageStat stat in stats)
            {
                if (stat.Downloads == lastCount)
                    continue;
                lastCount = stat.Downloads;
                sb.Append($"[\"{stat.Timestamp:s}\",{stat.Downloads}],");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');

            string json = sb.ToString();
            string filePath = $"packagestats/{packageName}.json";
            BlobClient blobClient = containerClient.GetBlobClient(filePath);
            using MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            blobClient.Upload(stream);
        }

        /// <summary>
        /// Given X/Y pairs, return only pairs where the Y value actually changed
        /// </summary>
        private static (double[] xs, double[] ys) UniquePoints(double[] xs, double[] ys)
        {
            List<double> xs2 = new List<double>();
            List<double> ys2 = new List<double>();

            xs2.Add(xs[0]);
            ys2.Add(ys[1]);

            for (int i = 1; i < ys.Length; i++)
            {
                if (ys2.Last() != ys[i])
                {
                    xs2.Add(xs[i]);
                    ys2.Add(ys[i]);
                }
            }

            return (xs2.ToArray(), ys2.ToArray());
        }
    }
}
