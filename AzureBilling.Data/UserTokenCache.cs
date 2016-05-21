using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBillingAPI.Data
{
    public class UserTokenCache : TableEntity
    {
        private const string PARTITION_KEY = "UserTokenCache";

        public UserTokenCache()
        {
            this.PartitionKey = PARTITION_KEY;
            this.RowKey = WebUserUniqueId;
        }
        
        public string WebUserUniqueId { get { return this.RowKey; } set { this.RowKey = value; } }
        public byte[] CacheBits { get; set; }
        public DateTime LastWrite { get; set; }
    }


}
