using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Drawing;
using Microsoft.Azure.Cosmos.Table;

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

            CloudTable tableNames = tableClient.GetTableReference("packageNames");
            string[] packageNames = GetPackageNames(tableNames, log);

            CloudTable tableStats = tableClient.GetTableReference("packageStats");
            DateTime timeStamp = DateTime.UtcNow;
            foreach (string packageName in packageNames)
            {
                PackageStat packageStat = new PackageStat()
                {
                    Package = packageName,
                    Downloads = RequestStats.WithJson(packageName),
                    Timestamp = timeStamp
                };

                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(packageStat);
                TableResult result = tableStats.Execute(insertOrMergeOperation);
                PackageStat insertedPackageStat = result.Result as PackageStat;
                log.LogInformation($"Inserted: {insertedPackageStat}");
            }
        }

        [FunctionName("UpdatePackagePlots")] // run on minute 2 of every hour
        public static void UpdatePackagePlots([TimerTrigger("0 2 * * * *")] TimerInfo myTimer, ILogger log)
        {
            /*
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("packageStats");
            string[] packageNames = GetPackageNames(table, log);
            */
        }

        private static string[] GetPackageNames(CloudTable table, ILogger log)
        {
            string[] packageNames = table.CreateQuery<PackageName>().Select(x => x.RowKey).ToArray();
            Array.Sort(packageNames);
            log.LogInformation($"Identified {packageNames.Length} packages: " + string.Join(", ", packageNames));
            return packageNames;
        }
    }
}
