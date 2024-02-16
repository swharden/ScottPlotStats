using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ScottPlotStats.Functions;

public class UpdateNugetStatsFunction(ILoggerFactory loggerFactory)
{
    private readonly ILogger Logger = loggerFactory.CreateLogger<UpdateNugetStatsFunction>();
    private const string DB_FILENAME = "scottplot.csv";
    private const string LOG_FILENAME = "scottplot-log.json";

    [Function("UpdateNugetStatsFunction")]
    public void Run(
#if DEBUG
    [TimerTrigger("0 0 0 1 1 0", RunOnStartup = false)] TimerInfo myTimer
#else
    [TimerTrigger("0 0 * * * *")] TimerInfo myTimer
#endif
        )
    {
        BlobStorage blob = new(Logger);

        string dbCsv = blob.ReadText(DB_FILENAME);
        CountDatabase db = CountDatabase.FromCsv(dbCsv);
        Logger.LogInformation("Loaded database with {COUNT} records.", db.RecordCount);

        db.AddRecord(NuGetAPI.GetPrimaryCount());
        db.AddRecord(NuGetAPI.GetSecondaryCount());
        Logger.LogInformation("Database has {COUNT} new records.", db.NewRecordCount);

        if (db.NewRecordCount > 0)
        {
            blob.SaveText(db.ToCSV(), DB_FILENAME);
            blob.SavePng(NuGetPlotting.DownloadCount(db), "scottplot-download-count.png", 600, 350);
            blob.SavePng(NuGetPlotting.DownloadRate(db), "scottplot-download-rate.png", 600, 350);
        }
        else
        {
            Logger.LogInformation("Database contains no updated records so the file will not be modified.");
        }

        blob.SaveText(db.GetLogJson(), LOG_FILENAME);
    }
}
