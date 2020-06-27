using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace PackagePopularityTracker
{
    public static class Functions
    {
        [FunctionName("UpdatePackageStats")] // run on minute 1 of every hour
        public static void UpdatePackageStats([TimerTrigger("0 1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var container = AzureBlob.GetStorageContainer("packagestats");
            string[] packageNames = GetPackageNames(container, log);
            UpdateStats(container, packageNames, log);
        }

        [FunctionName("UpdatePackagePlots")] // run on minute 2 of every hour
        public static void UpdatePackagePlots([TimerTrigger("0 2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var container = AzureBlob.GetStorageContainer("packagestats");
            string[] packageNames = GetPackageNames(container, log);
            var containerWeb = AzureBlob.GetStorageContainer("$web");
            UpdatePlots(containerWeb, packageNames, log);
        }

        private static string[] GetPackageNames(CloudBlobContainer container, ILogger log)
        {
            string[] packageNames = AzureBlob.ReadBlobText(container, "packages.txt")
                                             .Split(",")
                                             .Select(x => x.Trim())
                                             .ToArray();
            log.LogInformation($"Identified {packageNames.Length} packages: " + string.Join(", ", packageNames));
            return packageNames;
        }

        private static void UpdateStats(CloudBlobContainer container, string[] packageNames, ILogger log)
        {
            string dtStamp = DateTime.UtcNow.ToString("u");
            foreach (string packageName in packageNames)
            {
                int downloadCount = RequestStats.WithJson(packageName);
                log.LogInformation($"Package '{packageName}' has {downloadCount} downloads");
                string line = $"{dtStamp},{downloadCount}";
                AzureBlob.AppendBlob(container, $"{packageName}.csv", line);
            }
        }

        private static void UpdatePlots(CloudBlobContainer container, string[] packageNames, ILogger log)
        {
            foreach (string packageName in packageNames)
            {
                var plt = new ScottPlot.Plot();
                plt.Title($"{packageName} Popularity");
                plt.YLabel("Total Downloads");

                // TODO: read the blob for this file and plot the data

                Bitmap bmp = plt.GetBitmap(true);
                byte[] bmpBytes = ScottPlot.Tools.BitmapToBytes(bmp);
                log.LogInformation($"Plot for '{packageName}' has {bmpBytes.Length} bytes");

                var thisBlob = container.GetBlockBlobReference($"packagestats/{packageName}.png");
                thisBlob.UploadFromByteArray(bmpBytes, 0, bmpBytes.Length);
            }
        }
    }
}
