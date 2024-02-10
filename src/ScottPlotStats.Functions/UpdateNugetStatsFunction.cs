using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ScottPlotStats.Functions;

public class UpdateNugetStatsFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateNugetStatsFunction>();
    private const string DB_FILENAME = "scottplot.csv";

    [Function("UpdateNugetStatsFunction")]
    public void Run(
#if DEBUG
    [TimerTrigger("0 0 0 1 1 0", RunOnStartup = true)] TimerInfo myTimer
#else
    [TimerTrigger("0 0 * * * *")] TimerInfo myTimer
#endif
        )
    {
        BlobContainerClient containerClient = GetBlobContainer();
        CountDatabase db = LoadDatabaseFromFile(containerClient);
        db.AddRecord(NuGetAPI.GetPrimaryCount());
        db.AddRecord(NuGetAPI.GetSecondaryCount());
        SaveDatabaseToFile(db, containerClient);
        // TODO: update plot from database
        Logger.LogInformation("Function complete.");
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
        Logger.LogInformation("Database contains {COUNT} new records.", db.NewRecordCount);

        if (db.NewRecordCount == 0)
        {
            Logger.LogInformation("Database has no new records so it will not be updated.");
            return;
        }

        string txt = db.ToCSV();
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(txt);
        Logger.LogInformation("Writing {LENGTH} bytes...", bytes.Length);

        MemoryStream ms = new(bytes);
        BlobClient blobClient = containerClient.GetBlobClient(DB_FILENAME);
        blobClient.Upload(ms, overwrite: true);
        Logger.LogInformation("Upload complete.");
    }
}
