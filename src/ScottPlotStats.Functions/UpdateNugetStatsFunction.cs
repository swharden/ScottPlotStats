using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ScottPlotStats.Functions;

public class UpdateNugetStatsFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateNugetStatsFunction>();
    private const string DB_FILENAME = "scottplot.csv";
    private readonly Stopwatch SW = Stopwatch.StartNew();

    [Function("UpdateNugetStatsFunction")]
    public void Run(
#if DEBUG
    [TimerTrigger("0 0 0 1 1 0", RunOnStartup = true)] TimerInfo myTimer
#else
    [TimerTrigger("0 0 * * * *")] TimerInfo myTimer
#endif
        )
    {
        try
        {
            BlobContainerClient containerClient = GetBlobContainer();
            CountDatabase db = LoadDatabaseFromFile(containerClient);
            db.AddRecord(NuGetAPI.GetPrimaryCount());
            db.AddRecord(NuGetAPI.GetSecondaryCount());
            Logger.LogInformation("Database has {COUNT} new records.", db.NewRecordCount);

            bool alwaysUpdate = true; // disable this after testing is complete
            if (db.NewRecordCount > 0 || alwaysUpdate)
            {
                SaveDatabaseToFile(db, containerClient);
                CreatePlots(db, containerClient);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception thrown during stats update");
            throw;
        }
        finally
        {
            Logger.LogInformation("Function execution completed in {TIME} seconds", SW.Elapsed.TotalSeconds);
        }
    }

    private BlobContainerClient GetBlobContainer()
    {
        Logger.LogInformation("connecting to blob storage...");
        string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
            ?? throw new InvalidOperationException("connection string environment variable is missing");
        BlobServiceClient blobServiceClient = new(connectionString);
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("$web");
        Logger.LogInformation("connected.");
        return containerClient;
    }

    private CountDatabase LoadDatabaseFromFile(BlobContainerClient containerClient)
    {
        Logger.LogInformation("Downloading existing records from CSV file...");
        BlobClient blobClient = containerClient.GetBlobClient(DB_FILENAME);
        using MemoryStream memoryStream = new();
        blobClient.DownloadTo(memoryStream);
        string contentString = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
        Logger.LogInformation("Read {LENGTH} bytes.", contentString.Length);

        CountDatabase db = CountDatabase.FromCsv(contentString);
        Logger.LogInformation("Created database containing {COUNT} records.", db.Count);
        return db;
    }

    private void SaveDatabaseToFile(CountDatabase db, BlobContainerClient containerClient)
    {
        string txt = db.ToCSV();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(txt);
        Logger.LogInformation("Writing {LENGTH} bytes...", bytes.Length);

        using MemoryStream ms = new(bytes);
        BlobClient blobClient = containerClient.GetBlobClient(DB_FILENAME);
        blobClient.Upload(ms, overwrite: true);
        blobClient.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "text/plain" });
        Logger.LogInformation("Upload complete.");
    }

    private void CreatePlots(CountDatabase db, BlobContainerClient containerClient)
    {
        Logger.LogInformation("Plotting download count...");
        byte[] bytes1 = Plot.DownloadCount(db, 600, 350);
        Logger.LogInformation("Uploading {LENGTH} byte image file...", bytes1.Length);
        using MemoryStream ms1 = new(bytes1);
        BlobClient blobClient1 = containerClient.GetBlobClient("scottplot-download-count.png");
        blobClient1.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "image/png" });
        blobClient1.Upload(ms1, overwrite: true);

        Logger.LogInformation("Plotting download rate...");
        byte[] bytes2 = Plot.DownloadRate(db, 600, 350);
        Logger.LogInformation("Uploading {LENGTH} byte image file...", bytes2.Length);
        using MemoryStream ms2 = new(bytes2);
        BlobClient blobClient2 = containerClient.GetBlobClient("scottplot-download-rate.png");
        blobClient2.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "image/png" });
        blobClient2.Upload(ms2, overwrite: true);
    }
}
