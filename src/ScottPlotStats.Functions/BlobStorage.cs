using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace ScottPlotStats.Functions;

internal class BlobStorage
{
    readonly ILogger Logger;
    public readonly BlobContainerClient ContainerClient;

    public BlobStorage(ILogger logger, string containerName = "$web")
    {
        Logger = logger;
        Logger.LogInformation("connecting to blob storage...");
        string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
            ?? throw new InvalidOperationException("connection string environment variable is missing");
        BlobServiceClient blobServiceClient = new(connectionString);
        ContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
        Logger.LogInformation("connected.");
    }

    public void SavePng(ScottPlot.Plot plot, string filename, int width, int height)
    {
        byte[] bytes2 = plot.GetImageBytes(width, height, ScottPlot.ImageFormat.Png);
        Logger.LogInformation("Uploading {NAME} ({LENGTH} bytes)...", filename, bytes2.Length);

        using MemoryStream ms = new(bytes2);
        BlobClient client = ContainerClient.GetBlobClient(filename);
        client.Upload(ms, overwrite: true);
        BlobHttpHeaders headers = new() { ContentType = "image/png" };
        client.SetHttpHeaders(headers);
    }

    public void SaveText(string text, string filename)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
        SaveText(bytes, filename);
    }

    public void SaveText(byte[] bytes, string filename)
    {
        Logger.LogInformation("Uploading {NAME} ({LENGTH} bytes)...", filename, bytes.Length);
        using MemoryStream ms = new(bytes);
        BlobClient blobClient = ContainerClient.GetBlobClient(filename);
        blobClient.Upload(ms, overwrite: true);
        BlobHttpHeaders headers = new() { ContentType = "text/plain" };
        blobClient.SetHttpHeaders(headers);
    }

    public string ReadText(string filename)
    {
        Logger.LogInformation("Downloading {NAME}...", filename);
        BlobClient client = ContainerClient.GetBlobClient(filename);
        using MemoryStream ms = new();
        client.DownloadTo(ms);
        string str = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        Logger.LogInformation("Read {LENGTH} bytes.", str.Length);
        return str;
    }
}
