using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureBilling.Data
{
    public class UsageCommonProperties : TableEntity
    {
        public UsageCommonProperties()
        { }

        public UsageCommonProperties(string partitionKey, string rowKey)
        {
            this.RowKey = rowKey;
            this.PartitionKey = partitionKey;
        }

        public string Name { get; set; }

        public string Type { get; set; }

        public string SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        public string MeterId { get; set; }

        public double Quantity { get; set; }

        public string Unit { get; set; }

        public string MeterName { get; set; }

        public string MeterCategory { get; set; }

        public string MeterSubCategory { get; set; }

        public string MeterRegion { get; set; }

        public DateTime DetailsDateTime { get; set; }

        public string PullId { get; set; }

    }
}
