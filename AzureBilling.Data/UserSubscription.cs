using Microsoft.WindowsAzure.Storage.Table;

namespace AzureBilling.Data
{
    public class UserSubscription :TableEntity
    {
        private const string PARTITION_KEY = "UserSubscription";
        public UserSubscription() { }
        public UserSubscription(string id,string orgId)
        {
            this.RowKey = id;
            this.PartitionKey = PARTITION_KEY;
            this.OrganizationId = orgId;
        }
        public string SubscriptionId { get { return this.RowKey; } set { this.RowKey = value; } }

        public string OrganizationId { get; set; }

        public string DisplayName { get; set; }


        public string OfferId { get; set; }

        public string Currency { get; set; }

        public string RegionInfo { get; set; }
    }
}
