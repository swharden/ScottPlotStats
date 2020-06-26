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
using System.Data.SqlClient;

namespace PackagePopularityTracker
{
    public static class UpdatePackageStats
    {
        [FunctionName("UpdatePackageStats")] // run on minute 1 of every hour
        public static void Run([TimerTrigger("0 1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            Stopwatch sw = Stopwatch.StartNew();

            string packageList = ReadBlobText("packagestats", "packages.txt");
            string[] packageNames = packageList.Split('\n');

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < packageNames.Length; i++)
            {
                string packageName = packageNames[i].Trim();
                int downloadsApi = GetDownloadsFromJson(packageName);
                int downloadsWeb = GetDownloadsFromHtml(packageName);
                sb.AppendLine($"INSERT INTO PackageStatsAPI VALUES ('{DateTime.UtcNow}', '{packageName}', {downloadsApi});");
                sb.AppendLine($"INSERT INTO PackageStatsWeb VALUES ('{DateTime.UtcNow}', '{packageName}', {downloadsWeb});");
                log.LogInformation($"NuGet package '{packageName}' has {downloadsApi} API and {downloadsWeb} web downloads");
            }

            ExecuteSqlQuery(sb.ToString(), log);

            log.LogInformation($"Finished researching {packageNames.Length} packages in {sw.Elapsed} seconds");
        }

        private static void ExecuteSqlQuery(string query, ILogger log)
        {
            string sqlConnectionString = Environment.GetEnvironmentVariable("sqlConnString");
            using (SqlConnection conn = new SqlConnection(sqlConnectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    int modifiedRowCount = cmd.ExecuteNonQuery();
                    log.LogInformation($"modified {modifiedRowCount} database rows");
                }
            }
        }

        private static string ReadBlobText(string containerName, string fileName)
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

        private static int GetDownloadsFromHtml(string packageName)
        {
            string url = "https://www.nuget.org/packages/" + packageName;
            using var webClient = new System.Net.WebClient();
            string[] lines = webClient.DownloadString(url).Split(Environment.NewLine);
            for (int i = 0; i < lines.Length; i++)
            {
                string thisLine = lines[i].Trim();
                if (thisLine.EndsWith("total downloads"))
                {
                    thisLine = thisLine.Replace("total downloads", "");
                    bool isSuccessfulParse = int.TryParse(thisLine, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out int downloadCount);
                    if (isSuccessfulParse)
                        return downloadCount;
                }
            }
            return -1;
        }

        private static int GetDownloadsFromJson(string packageName)
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
    }
}
