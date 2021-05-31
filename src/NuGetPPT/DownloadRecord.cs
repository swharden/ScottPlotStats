using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NuGetPPT
{
    public class DownloadRecord : IComparable<DownloadRecord>
    {
        public readonly string Timestamp;
        public readonly int Downloads;
        public readonly DateTime DateTime;
        public DownloadRecord(string timestamp, int downloads)
        {
            Timestamp = timestamp;
            Downloads = downloads;
            DateTime = DateTime.Parse(timestamp);
        }

        public int CompareTo(DownloadRecord other)
        {
            if (DateTime > other.DateTime)
                return 1;
            else if (DateTime < other.DateTime)
                return -1;
            else
                return 0;
        }
    }
}
