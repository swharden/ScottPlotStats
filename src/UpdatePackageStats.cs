using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Microsoft.Azure;
using Microsoft.Azure.Storage.RetryPolicies;

namespace PackagePopularityTracker
{
    public static class UpdatePackageStats
    {
        [FunctionName("UpdatePackageStats")]
        public static void Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer, ILogger log)
        {
            Stopwatch sw = Stopwatch.StartNew();

            StringBuilder sb = new StringBuilder();

            string timeStamp = DateTime.UtcNow.AddSeconds(5).ToString("u");
            sb.Append($"{timeStamp},");

            string packageList = ReadBlobText("packagestats", "packages.txt");
            string[] packageNames = packageList.Split(Environment.NewLine);
            foreach (string packageName in packageNames)
            {
                string trimmedPackageName = packageName.Trim();
                int downloadCount = LookupDownloadCount(trimmedPackageName);
                sb.Append($"{trimmedPackageName}={downloadCount},");
            }

            string statsLine = sb.ToString().Trim(',');
            log.LogInformation(statsLine);
            AppendBlob("packagestats", "downloads.txt", statsLine);

            log.LogInformation($"Looked-up {packageNames.Length} packages in {sw.Elapsed} seconds");

        }

        private static int LookupDownloadCount(string packageName)
        {
            string url = "https://azuresearch-usnc.nuget.org/query?take=1&q=" + packageName;

            using var webClient = new System.Net.WebClient();

            byte[] bytes = webClient.DownloadData(url);
            var jsonReader = JsonReaderWriterFactory.CreateJsonReader(bytes, new System.Xml.XmlDictionaryReaderQuotas());
            XElement element = XElement.Load(jsonReader).XPathSelectElement("//data").Elements().First();

            string packageName2 = element.XPathSelectElement("title").Value;
            if (packageName != packageName2)
                throw new InvalidOperationException($"package name '{packageName2}' did not match '{packageName}'");

            int downloads = int.Parse(element.XPathSelectElement("totalDownloads").Value);
            return downloads;
        }

        public static string ReadBlobText(string containerName, string fileName)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
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

        public static void AppendBlob(string containerName, string fileName, string txt, bool lineBreakBefore = true)
        {
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudAppendBlob appendBlob = container.GetAppendBlobReference(fileName);
            if (!appendBlob.Exists())
                appendBlob.CreateOrReplace();
            if (lineBreakBefore)
                appendBlob.AppendText("\n");
            appendBlob.AppendText(txt);
        }
    }
}
