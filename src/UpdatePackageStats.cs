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
            using SqlConnection conn = new SqlConnection(Environment.GetEnvironmentVariable("sqlConnString"));
            conn.Open();

            // read package list from the database
            var packageNames = new List<string>();
            using (SqlCommand sqlCmd = new SqlCommand("SELECT * FROM PackageStatsPackages", conn))
            using (SqlDataReader reader = sqlCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string packageName = (string)reader["PackageName"];
                    packageNames.Add(packageName);
                }
            }
            log.LogInformation($"Database listed {packageNames.Count()} packages: " + string.Join(", ", packageNames));

            // look up downloads for each package and build a query
            string timestamp = DateTime.UtcNow.ToString();
            StringBuilder sb = new StringBuilder();
            foreach (var packageName in packageNames)
            {
                int downloadsApi = GetDownloadsFromJson(packageName);
                int downloadsWeb = GetDownloadsFromHtml(packageName);
                sb.AppendLine($"INSERT INTO PackageStatsAPI VALUES ('{timestamp}', '{packageName}', {downloadsApi});");
                sb.AppendLine($"INSERT INTO PackageStatsWeb VALUES ('{timestamp}', '{packageName}', {downloadsWeb});");
                log.LogInformation($"NuGet package '{packageName}' has {downloadsApi} API and {downloadsWeb} web downloads");
            }

            // execute the query
            using (SqlCommand sqlCmd = new SqlCommand(sb.ToString(), conn))
            {
                int affectedRowCount = sqlCmd.ExecuteNonQuery();
                log.LogInformation($"affected {affectedRowCount} database rows");
            }
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
