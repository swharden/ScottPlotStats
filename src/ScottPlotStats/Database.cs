namespace ScottPlotStats;

public static class Database
{
    public static List<CountRecord> GetRecords(string text)
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
}
