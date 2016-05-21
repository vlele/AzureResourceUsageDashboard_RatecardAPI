using System.Collections.Generic;

namespace AzureBilling.WebJob
{
    public class MeterData
    {
        public string MeterId { get; set; }

        public double IncludedQuantity { get; set; }

        public Dictionary<double, double> TieredRates { get; set; }
    }

}
