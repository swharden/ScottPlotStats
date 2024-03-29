﻿using System.Collections;
using System.Text;
using System.Text.Json;

namespace ScottPlotStats;

public class CountDatabase
{
    private readonly List<CountRecord> Records = [];
    private int InitialCount;
    public int RecordCount => Records.Count;
    public int NewRecordCount => RecordCount - InitialCount;
    public int HighestCount => Records.Select(x => x.Count).Max();
    public CountRecord[] GetRecords() => Records.ToArray();

    private CountDatabase()
    {

    }

    public static CountDatabase FromCsv(string text)
    {
        CountDatabase db = new();
        var records = GetRecordsFromCsv(text);
        db.Records.AddRange(records);
        db.InitialCount = records.Count;
        return db;
    }

    public override string ToString()
    {
        return $"Database with {RecordCount} records ({RecordCount - InitialCount} new ones)";
    }

    public void AddRecord(CountRecord newRecord)
    {
        var lastRecord = Records.Where(x => x.Server == newRecord.Server).LastOrDefault();
        int lastCount = lastRecord is null ? 0 : lastRecord.Count;
        if (newRecord.Count > lastCount)
        {
            Records.Add(newRecord);
        }
    }

    private static List<CountRecord> GetRecordsFromCsv(string text)
    {
        List<CountRecord> records = [];

        string[] lines = text.Split("\n");
        foreach (string line in lines)
        {
            if (line.StartsWith('#'))
                continue;

            string[] parts = line.Split(",");
            if (parts.Length != 3)
                continue;

            DateTime dt = DateTime.Parse(parts[0]).ToUniversalTime();
            int server = int.Parse(parts[1]);
            int count = int.Parse(parts[2]);

            CountRecord log = new(dt, server, count);
            records.Add(log);
        }

        return records;
    }

    public string ToCSV()
    {
        StringBuilder sb = new();
        sb.AppendLine("# Date, Server, Count");
        foreach (CountRecord record in Records)
        {
            sb.AppendLine(record.ToCsvLine());
        }
        return sb.ToString();
    }

    public string GetLogJson()
    {
        using MemoryStream ms1 = new();
        JsonWriterOptions options = new() { Indented = true };
        using Utf8JsonWriter writer = new(ms1, options);
        writer.WriteStartObject();
        writer.WriteString("updated", DateTime.Now.ToUniversalTime().ToString("o"));
        writer.WriteString("last-new-record", GetRecords().Last().Date.ToUniversalTime().ToString("o"));
        writer.WriteNumber("total", HighestCount);
        writer.WriteEndObject();
        writer.Flush();
        writer.Flush();
        byte[] bytes = ms1.ToArray();
        return Encoding.UTF8.GetString(bytes);
    }
}
