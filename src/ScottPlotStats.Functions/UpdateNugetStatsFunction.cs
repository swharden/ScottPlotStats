using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace ScottPlotStats.Functions;

public class UpdateNugetStatsFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateNugetStatsFunction>();
    private const string DB_FILENAME = "scottplot.csv";
    private const string LOG_FILENAME = "scottplot-log.json";
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
            UpdateDatabaseWithLatestCounts(db);
            SaveDatabaseToFile(db, containerClient);
            CreatePlots(db, containerClient);
            CreateLogFile(db, containerClient);
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
        Logger.LogInformation("Created database containing {COUNT} records.", db.RecordCount);
        return db;
    }

    private void UpdateDatabaseWithLatestCounts(CountDatabase db)
    {
        db.AddRecord(NuGetAPI.GetPrimaryCount());
        db.AddRecord(NuGetAPI.GetSecondaryCount());
        Logger.LogInformation("Database has {COUNT} new records.", db.NewRecordCount);
    }

    private void SaveDatabaseToFile(CountDatabase db, BlobContainerClient containerClient)
    {
        if (db.NewRecordCount == 0)
        {
            Logger.LogInformation("Database contains no updated records so the file will not be modified.");
            return;
        }

        string txt = db.ToCSV();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(txt);
        Logger.LogInformation("Writing {LENGTH} bytes...", bytes.Length);

        using MemoryStream ms = new(bytes);
        BlobClient blobClient = containerClient.GetBlobClient(DB_FILENAME);
        blobClient.Upload(ms, overwrite: true);
        blobClient.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "text/plain" });
    }

    private void CreatePlots(CountDatabase db, BlobContainerClient containerClient)
    {
        if (db.NewRecordCount == 0)
        {
            Logger.LogInformation("Database contains no updated records so plots will not be updated.");
            return;
        }

        Logger.LogInformation("Plotting download count...");
        byte[] bytes1 = Plot.DownloadCount(db, 600, 350);
        Logger.LogInformation("Uploading {LENGTH} byte image file...", bytes1.Length);
        using MemoryStream ms1 = new(bytes1);
        BlobClient blobClient1 = containerClient.GetBlobClient("scottplot-download-count.png");
        blobClient1.Upload(ms1, overwrite: true);
        blobClient1.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "image/png" });

        Logger.LogInformation("Plotting download rate...");
        byte[] bytes2 = Plot.DownloadRate(db, 600, 350);
        Logger.LogInformation("Uploading {LENGTH} byte image file...", bytes2.Length);
        using MemoryStream ms2 = new(bytes2);
        BlobClient blobClient2 = containerClient.GetBlobClient("scottplot-download-rate.png");
        blobClient2.Upload(ms2, overwrite: true);
        blobClient2.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "image/png" });
    }

    private void CreateLogFile(CountDatabase db, BlobContainerClient containerClient)
    {
        using MemoryStream ms1 = new();
        JsonWriterOptions options = new() { Indented = true };
        using Utf8JsonWriter writer = new(ms1, options);
        writer.WriteStartObject();
        writer.WriteString("updated", DateTime.Now.ToUniversalTime().ToString("o"));
        writer.WriteString("last-new-record", db.GetRecords().Last().Date.ToUniversalTime().ToString("o"));
        writer.WriteNumber("total", db.HighestCount);
        writer.WriteEndObject();
        writer.Flush();
        writer.Flush();
        byte[] bytes = ms1.ToArray();

        Logger.LogInformation("Writing {LENGTH} byte log file...", bytes.Length);
        using MemoryStream ms2 = new(bytes);
        BlobClient blobClient = containerClient.GetBlobClient(LOG_FILENAME);
        blobClient.Upload(ms2, overwrite: true);
        blobClient.SetHttpHeaders(new BlobHttpHeaders() { ContentType = "text/plain" });
    }
}
