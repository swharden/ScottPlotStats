namespace ScottPlotStats;

public record CountRecord(DateTime Date, int Server, int Count)
{
    public override string ToString()
    {
        return $"{Date:o} count={Count:N0} (server {Server})";
    }

    public string ToCsvLine()
    {
        return $"{Date:o},{Server},{Count}";
    }
}