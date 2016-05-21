using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBilling.Data
{
    public class AzureUsageDetailsMeterAggregate : UsageCommonProperties
    {
        public double Amount { get; set; }

        public AzureUsageDetailsMeterAggregate() { }

        public AzureUsageDetailsMeterAggregate(string partitionKey, string rowKey) : base(partitionKey, rowKey) { }
    }
}
