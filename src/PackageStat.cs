using Microsoft.Azure.Cosmos.Table;
using System;

namespace PackagePopularityTracker
{
    public class PackageStat : TableEntity
    {
        public string Package { get; set; }
        public int Downloads { get; set; }
        public PackageStat()
        {
            PartitionKey = "packageStat";
            RowKey = Guid.NewGuid().ToString();
        }
        public override string ToString()
        {
            return $"[{Timestamp}] Package '{Package}' has {Downloads} downloads";
        }
    }
}
