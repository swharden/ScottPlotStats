using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PackagePopularityTracker
{
    public static class RequestStats
    {
        public static int WithHtml(string packageName)
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

        public static int WithJson(string packageName)
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
