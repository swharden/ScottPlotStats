using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos.Table;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PackagePopularityTracker.Functions
{
    public static class Stats
    {
        const string OncePerHour = "0 5 * * * *"; // 5 minutes after every hour

        /// <summary>
        /// This method reads the latest total download counts and logs findings to tabular storage.
        /// </summary>
        [FunctionName("UpdatePackageStats")]
        public static void UpdatePackageStats([TimerTrigger(OncePerHour)] TimerInfo _, ILogger log)
        {
            // connect to tabular storage
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            DateTime timeStamp = DateTime.UtcNow;

            // read package names from tabular storage
            CloudTable namesTable = tableClient.GetTableReference("packageNames");
            string[] packageNames = namesTable.CreateQuery<PackageName>().Select(x => x.RowKey).ToArray();

            // record stats for each package
            CloudTable statsTable = tableClient.GetTableReference("packageStats");
            foreach (string packageName in packageNames)
            {
                PackageStat packageStat = new PackageStat()
                {
                    Package = packageName,
                    Downloads = HttpRequestDownloadCount(packageName),
                    Timestamp = timeStamp
                };

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(packageStat);
                TableResult result = statsTable.Execute(insertOrMergeOperation);
                PackageStat insertedPackageStat = result.Result as PackageStat;
                log.LogInformation($"Inserted: {insertedPackageStat}");
            }
        }

        /// <summary>
        /// Structure for the data stored in the package names table
        /// </summary>
        private class PackageName : TableEntity
        {
            public PackageName()
            {
                PartitionKey = "packageName";
            }
        }

        /// <summary>
        /// Hit the NuGet API to get download stats in JSON format
        /// </summary>
        public static int HttpRequestDownloadCount(string packageName)
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
