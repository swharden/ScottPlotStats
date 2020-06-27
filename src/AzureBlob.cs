using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
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

        // read a blog file into a string
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

        // append a string to an AppendBlob
        public static void AppendBlob(CloudBlobContainer container, string fileName, string txt, bool lineBreakBefore = true)
        {
            if (lineBreakBefore)
                txt = "\n" + txt;

            CloudAppendBlob appendBlob = container.GetAppendBlobReference(fileName);

            if (!appendBlob.Exists())
                appendBlob.CreateOrReplace();

            appendBlob.AppendText(txt);
        }
    }
}
