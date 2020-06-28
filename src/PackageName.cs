using Microsoft.Azure.Cosmos.Table;
using System;

namespace PackagePopularityTracker
{
    public class PackageName : TableEntity
    {
        public PackageName()
        {
            PartitionKey = "packageName";
        }
    }
}
