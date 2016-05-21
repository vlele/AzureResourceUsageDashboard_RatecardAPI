using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureBilling.Data
{
    public class Logger
    {
        public static void Log(string source, string type, string message, string detail)
        {
            EntityRepo<ErrorLogEntity> repo = new EntityRepo<ErrorLogEntity>();
            repo.Insert(new System.Collections.Generic.List<ErrorLogEntity>
            {
                new ErrorLogEntity(source, type, message, detail)
            });
        }
    }
}
