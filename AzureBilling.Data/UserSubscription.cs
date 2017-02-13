using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;

namespace AzureBilling.Data
{
    public class UserSubscription
    {
        public List<Subscription> Subscriptions { get; set; }
    }

    public class Subscription
    {
        public string SubscriptionId { get; set; }

        public string OrganizationId { get; set; }

        public string DisplayName { get; set; }

        public string OfferId { get; set; }

        public string Currency { get; set; }

        public string RegionInfo { get; set; }
    }
}
