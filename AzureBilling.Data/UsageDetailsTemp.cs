using System.Collections.Generic;

namespace AzureBillingAPI.Data
{
    public class UsageDetailsTemp
    {
        public List<Value> value { get; set; }
    }

    public class InfoFields
    {
        public string meteredRegion { get; set; }
        public string meteredService { get; set; }
        public string meteredServiceType { get; set; }
        public string project { get; set; }
    }

    public class Properties
    {
        public string subscriptionId { get; set; }
        public string usageStartTime { get; set; }
        public string usageEndTime { get; set; }
        public string meterName { get; set; }
        public string meterCategory { get; set; }
        public string meterSubCategory { get; set; }
        public string meterRegion { get; set; }
        public string unit { get; set; }
        public string instanceData { get; set; }
        public string meterId { get; set; }
        public InfoFields infoFields { get; set; }
        public double quantity { get; set; }
    }

    public class Value
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
    }
}
