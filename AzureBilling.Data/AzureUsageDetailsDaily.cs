using System;

namespace AzureBilling.Data
{
    public class AzureUsageDetailsDaily : UsageCommonProperties
    {
        public DateTime UsageStartTime { get; set; }

        public DateTime UsageEndTime { get; set; }

        public double Amount { get; set; }

        public AzureUsageDetailsDaily() { }

        public AzureUsageDetailsDaily(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }

    }
}
