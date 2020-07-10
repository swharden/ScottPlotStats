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

namespace PackagePopularityTracker
{
    public static class Functions
    {
        [FunctionName("UpdatePackageStats")] // run on minute 1 of every hour
        public static void UpdatePackageStats([TimerTrigger("0 1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            DateTime timeStamp = DateTime.UtcNow;

            // read package names
            CloudTable namesTable = tableClient.GetTableReference("packageNames");
            string[] packageNames = namesTable.CreateQuery<PackageName>().Select(x => x.RowKey).ToArray();

            // record stats for each package
            CloudTable statsTable = tableClient.GetTableReference("packageStats");
            foreach (string packageName in packageNames)
            {
                PackageStat packageStat = new PackageStat()
                {
                    Package = packageName,
                    Downloads = RequestStats.WithJson(packageName),
                    Timestamp = timeStamp
                };

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(packageStat);
                TableResult result = statsTable.Execute(insertOrMergeOperation);
                PackageStat insertedPackageStat = result.Result as PackageStat;
                log.LogInformation($"Inserted: {insertedPackageStat}");
            }
        }

        public static (double[] xs, double[] ys) UniquePoints(double[] xs, double[] ys)
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

        [FunctionName("UpdatePackagePlots")] // run on minute 2 of every hour
        public static void UpdatePackagePlots([TimerTrigger("0 2 * * * *")] TimerInfo myTimer, ILogger log)
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
                PackageStat[] stats = allStats.Where(x => x.Package == packageName).ToArray();
                log.LogInformation($"Package {packageName} has {stats.Length} records.");
                double[] datetimes = stats.Select(x => x.Timestamp.DateTime.ToOADate()).ToArray();
                double[] downloads = stats.Select(x => (double)x.Downloads).ToArray();

                var unique = UniquePoints(datetimes, downloads);

                var plt = new ScottPlot.Plot(600, 400);
                plt.Title(packageName);
                plt.YLabel("Downloads");
                //plt.PlotScatter(datetimes, downloads, lineWidth: 0);
                //plt.PlotStep(unique.xs, unique.ys);
                plt.PlotScatter(unique.xs, unique.ys, lineWidth: 2);
                plt.Ticks(dateTimeX: true);

                string filePath = $"packagestats/{packageName}.png";
                BlobClient blobClient = containerClient.GetBlobClient(filePath);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Bitmap bmp = plt.GetBitmap(true);
                    bmp.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    blobClient.Upload(memoryStream, overwrite: true);
                }

                log.LogInformation($"Created {filePath}");
            }
        }
    }
}
