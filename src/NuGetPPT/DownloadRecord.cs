using System;
using System.Collections.Generic;
using System.Text;

namespace NuGetPPT
{
    public class DownloadRecord
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
    }
}
