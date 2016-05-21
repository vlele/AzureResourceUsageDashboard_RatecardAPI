using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureBilling.Data
{
    public class ErrorLogEntity : TableEntity
    {
        public ErrorLogEntity() { }
        public ErrorLogEntity(string source, string logType, string message, string details)
        {
            this.RowKey = logType + "_" + Guid.NewGuid().ToString();
            this.PartitionKey = source;
            this.Message = message;
        }

        public string Message { get; set; }

        public string LogType { get; set; }
    }
}
