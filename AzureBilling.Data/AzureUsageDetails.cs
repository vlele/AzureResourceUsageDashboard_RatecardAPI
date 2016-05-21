using System;

namespace AzureBilling.Data
{
    public class AzureUsageDetails :UsageCommonProperties
    {
        public string InstanceId { get; set; }

        public DateTime UsageStartTime { get; set; }

        public DateTime UsageEndTime { get; set; }

        public AzureUsageDetails(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }

        public AzureUsageDetails() { }
    }
}
