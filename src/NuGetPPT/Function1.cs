using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace NuGetPPT
{
    public static class Function1
    {
        [FunctionName("UpdateStats")]
        public static void Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // TODO: read this from a configuration file in blob storage?
            string[] packageNames = { "scottplot", "scottplot.winforms", "scottplot.wpf", "scottplot.avalonia", "spectrogram", "fftsharp" };

            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            BlobContainerClient container = new(storageConnectionString, "$web");

            foreach (string packageName in packageNames)
            {
                List<DownloadRecord> records = LoadRecords(container, packageName);
                records.Sort();
                int oldDownloads = records.Last().Downloads;
                Console.WriteLine($"{packageName} has {records.Count} records (latest {oldDownloads} downloads)");
                int newDownloads = NuGetApi.ReadTotalDownloads(NuGetApi.RequestJson(packageName));
                if (oldDownloads == newDownloads)
                {
                    Console.WriteLine($"{packageName} download count has not changed");
                }
                else
                {
                    Console.WriteLine($"{packageName} now has {newDownloads} downloads");
                    records.Add(new DownloadRecord(DateTime.UtcNow.ToString(), newDownloads));
                    SaveRecords(container, packageName, records);
                }
                Console.WriteLine();
            }
        }

        private static List<DownloadRecord> LoadRecords(BlobContainerClient container, string packageName)
        {
            string fileName = $"logs/{packageName}.json";
            BlobClient blob = container.GetBlobClient(fileName);
            if (!blob.Exists())
                throw new InvalidOperationException($"file not found: {fileName}");
            using MemoryStream stream = new();
            blob.DownloadTo(stream);
            string json = Encoding.UTF8.GetString(stream.ToArray());
            return IO.FromJson(json);
        }

        private static void SaveRecords(BlobContainerClient container, string packageName, List<DownloadRecord> records)
        {
            string fileName = $"logs/{packageName}.json";
            BlobClient blob = container.GetBlobClient(fileName);
            string json = IO.ToJson(records, packageName);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using var stream = new MemoryStream(bytes, writable: false);
            blob.Upload(stream, overwrite: true);
            stream.Close();
        }
    }
}
