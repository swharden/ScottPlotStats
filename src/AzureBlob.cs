using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace PackagePopularityTracker
{
    public static class AzureBlob
    {
        public static CloudBlobContainer GetStorageContainer(string containerName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            return container;
        }

        public static string ReadBlobText(CloudBlobContainer container, string fileName)
        {
            var thisBlob = container.GetBlockBlobReference(fileName);

            using var memoryStream = new MemoryStream();

            var blobRequestOptions = new BlobRequestOptions
            {
                ServerTimeout = TimeSpan.FromSeconds(30),
                MaximumExecutionTime = TimeSpan.FromSeconds(120),
                RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(3), maxAttempts: 3),
            };

            thisBlob.DownloadToStream(memoryStream, null, blobRequestOptions);
            byte[] bytes = memoryStream.ToArray();
            string text = Encoding.Default.GetString(bytes);

            return text;
        }

        public static void AppendBlob(CloudBlobContainer container, string fileName, string txt, bool lineBreakBefore = true)
        {
            if (lineBreakBefore)
                txt = "\n" + txt;

            CloudAppendBlob appendBlob = container.GetAppendBlobReference(fileName);

            if (!appendBlob.Exists())
                appendBlob.CreateOrReplace();

            appendBlob.AppendText(txt);
        }

        public static void UpdatePlots(CloudBlobContainer container, string[] packageNames, ILogger log)
        {
            foreach (string packageName in packageNames)
            {
                var plt = new ScottPlot.Plot();
                plt.Title($"{packageName} Popularity");
                plt.YLabel("Total Downloads");

                // TODO: plot the data

                Bitmap bmp = plt.GetBitmap(true);
                byte[] bmpBytes = ScottPlot.Tools.BitmapToBytes(bmp);
                log.LogInformation($"Plot for '{packageName}' has {bmpBytes.Length} bytes");

                var thisBlob = container.GetBlockBlobReference($"packagestats/{packageName}.png");
                thisBlob.UploadFromByteArray(bmpBytes, 0, bmpBytes.Length);
            }
        }
    }
}
