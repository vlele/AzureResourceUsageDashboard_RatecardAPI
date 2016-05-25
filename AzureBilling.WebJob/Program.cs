using AzureBilling.Data;
using AzureBilling.WebJob;
using AzureBillingAPI.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace AzureUsageDataImport
{
    class Program
    {
        private const string USER_SUBSCRIPTION_TABLE_PARTITION_KEY = "UserSubscription";

        static void Main(string[] args)
        {
            try
            {
                string baseUrl = ConfigurationManager.AppSettings["API-URL"];
                PullUsageAndBillingData(baseUrl);
            }
            catch (Exception exp)
            {
                Logger.Log("WebJob", "Error", exp.Message, exp.ToString());
                throw;
            }
        }

        private static void PullUsageAndBillingData(string baseUrl)
        {
            string runId = "";
            DateTime jobStartTime = DateTime.UtcNow;
            DateTime jobEndTime = DateTime.UtcNow;

            var missingSubscriptionIds = new List<string>();

            // pull subscription data from appsettings
            string subscriptionJson = ConfigurationManager.AppSettings["Subscriptions"].ToString();
            var subscriptionList = JsonConvert.DeserializeObject<UserSubscription>(subscriptionJson);

            foreach (var item in subscriptionList.Subscriptions)
            {
                jobStartTime = DateTime.UtcNow;
                string subscriptionId = item.SubscriptionId;
                string organizationId = item.OrganizationId;
                string offerId = item.OfferId;
                string currency = item.Currency;
                string regionInfo = item.RegionInfo;
                string subscriptionName = item.DisplayName;

                // pulling the month to date data
                // including the last month last date as well
                var today = DateTime.UtcNow;
                var currentMonth = new DateTime(today.Year, today.Month, 1);
                var lastDateOfPreviousMonth = currentMonth.AddDays(-1);
                string endTime = GetDateString(today);
                string startTime = GetDateString(lastDateOfPreviousMonth);

                // if subscription data is incomplete, skip the whole data load process
                if (!IsValidData(subscriptionId, organizationId, offerId, currency, regionInfo))
                {
                    missingSubscriptionIds.Add(subscriptionId);
                    continue;
                }

                runId = Guid.NewGuid().ToString();
                string language = ConfigurationManager.AppSettings["Language"];
                // get rate card data
                Dictionary<string, MeterData> meterRateDictionary = GetMeterData(subscriptionId, organizationId, offerId, currency, language, regionInfo, baseUrl);

                // get usage data
                GetUsageData(meterRateDictionary, subscriptionName, subscriptionId, organizationId, startTime, endTime, runId, baseUrl);
                jobEndTime = DateTime.UtcNow;

            }

            // log message for yet to be configured subscriptions
            if (missingSubscriptionIds.Count > 0)
            {
                string message = "USER ACTION REQUIRED FOR Subscription(s): " + string.Join(",", missingSubscriptionIds.ToArray()) + " Visit Dashboard, go to 'My Subscription' and fill up details.";
                LogWebJobRunInfo(message, jobStartTime, jobEndTime);
            }

            // log the run info
            if (!string.IsNullOrEmpty(runId))
            {
                LogWebJobRunInfo(runId, jobStartTime, jobEndTime);
            }
        }

        private static void LogWebJobRunInfo(string message, DateTime jobStartTime, DateTime jobEndTime)
        {
            string monthStr = DateTime.UtcNow.Month < 10 ? "0" + DateTime.UtcNow.Month.ToString() : DateTime.UtcNow.Month.ToString();
            string yearStr = DateTime.UtcNow.Year.ToString();

            // Insert RunId after each successful run
            EntityRepo<WebJobRunInfo> repo = new EntityRepo<WebJobRunInfo>();
            repo.Insert(new List<WebJobRunInfo>() {
                new WebJobRunInfo {
                    PartitionKey = yearStr+"-"+monthStr,
                    RowKey = Guid.NewGuid().ToString(),
                    RunId = message,
                    StartTimeUTC = jobStartTime,
                    EndTimeUTC = jobEndTime
                }
            });
        }

        private static string GetDateString(DateTime utcNow)
        {
            var month = utcNow.Month;
            var day = utcNow.Day;
            return string.Format("{0}-{1}-{2}", utcNow.Year, month < 10 ? "0" + month.ToString() : month.ToString(), day < 10 ? "0" + day.ToString() : day.ToString());
        }

        private static bool IsValidData(string subscriptionId, string organizationId, string offerId, string currency, string regionInfo)
        {
            return !string.IsNullOrEmpty(subscriptionId)
                && !string.IsNullOrEmpty(organizationId)
                && !string.IsNullOrEmpty(offerId)
                && !string.IsNullOrEmpty(currency)
                && !string.IsNullOrEmpty(regionInfo);
        }

        private static void GetUsageData(Dictionary<string, MeterData> meterRateDictionary, string subscriptionName, string subscriptionId, string organizationId, string startTime, string endTime, string pullId, string baseUrl)
        {
            string requestUrl = String.Format("{0}/ratecard/GetUsageData?subscriptionId={1}&organizationId={2}&startDate={3}&endDate={4}",
               baseUrl,
               subscriptionId,
               organizationId,
              startTime,
              endTime);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            // Read Response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();

            // read stream as text
            StreamReader responseStream = new StreamReader(receiveStream);
            string usageText = responseStream.ReadToEnd();

            UsageDetailsTemp usage = JsonConvert.DeserializeObject<UsageDetailsTemp>(usageText);
            List<AzureUsageDetails> usageDetails = new List<AzureUsageDetails>();
            DateTime detailsDateTime = DateTime.Now;
            if (usage != null && usage.value != null)
            {
                foreach (Value val in usage.value)
                {
                    var instanceId = "None";
                    if (val.properties.instanceData != null)
                    {
                        var instanceKey = JToken.Parse(val.properties.instanceData);
                        instanceId = (string)instanceKey["Microsoft.Resources"]["resourceUri"];
                    }

                    var aggregateKey = val.properties.usageStartTime + "_" + val.properties.usageEndTime;
                    var rowKey = GetInstanceBasedRowKey(val.properties.subscriptionId, val.properties.meterId, instanceId, aggregateKey);
                    var partitionKey = GetMonthBasedPartitionKey(Convert.ToDateTime(val.properties.usageEndTime));
                    AzureUsageDetails newDetail = GetInstanceLevelUsageDetail(detailsDateTime, subscriptionName, pullId, val, instanceId, rowKey, partitionKey);
                    usageDetails.Add(newDetail);
                }
            }
            EntityRepo<AzureUsageDetails> usageEntityRepo = new EntityRepo<AzureUsageDetails>();
            usageEntityRepo.Insert(usageDetails);

            IEnumerable<AzureUsageDetailsDaily> aggregateUsage = AggregateAtDailyUsage(usageDetails,meterRateDictionary);
            EntityRepo<AzureUsageDetailsDaily> usageEntityRepoAgg = new EntityRepo<AzureUsageDetailsDaily>();
            usageEntityRepoAgg.Insert(aggregateUsage.ToList());

            IEnumerable<AzureUsageDetailsMeterAggregate> aggregateSubscription = AggregateAtMeterWithExpense(meterRateDictionary, usageDetails);
            EntityRepo<AzureUsageDetailsMeterAggregate> aggregateSubscriptionRepo = new EntityRepo<AzureUsageDetailsMeterAggregate>();
            aggregateSubscriptionRepo.Insert(aggregateSubscription.ToList());
        }

        private static AzureUsageDetails GetInstanceLevelUsageDetail(DateTime detailsDateTime, string subscriptionName, string pullId, Value val, string instanceId, string rowKey, string partitionKey)
        {
            AzureUsageDetails newDetail = new AzureUsageDetails(partitionKey, rowKey);
            newDetail.InstanceId = instanceId;
            newDetail.Name = val.name;
            newDetail.Type = val.type;
            newDetail.SubscriptionId = val.properties.subscriptionId;
            newDetail.SubscriptionName = subscriptionName;
            newDetail.UsageStartTime = Convert.ToDateTime(val.properties.usageStartTime);
            newDetail.UsageEndTime = Convert.ToDateTime(val.properties.usageEndTime);
            newDetail.Quantity = val.properties.quantity;
            newDetail.Unit = val.properties.unit;
            newDetail.MeterId = val.properties.meterId;
            newDetail.MeterCategory = val.properties.meterCategory;
            newDetail.MeterSubCategory = val.properties.meterSubCategory;
            newDetail.MeterName = val.properties.meterName;
            newDetail.MeterRegion = val.properties.infoFields.meteredRegion;
            newDetail.DetailsDateTime = detailsDateTime;
            newDetail.PullId = pullId;
            return newDetail;
        }

        private static IEnumerable<AzureUsageDetailsMeterAggregate> AggregateAtMeterWithExpense(Dictionary<string, MeterData> meterRateDictionary, List<AzureUsageDetails> usageDetails)
        {
            return from us in usageDetails
                   group us by new
                   {
                       us.MeterId,
                       us.MeterName,
                       us.MeterCategory,
                       us.MeterSubCategory,
                       us.SubscriptionId,
                       us.SubscriptionName,
                       us.Unit,
                       us.PartitionKey
                   }
                                               into fus
                   select new AzureUsageDetailsMeterAggregate()
                   {
                       PartitionKey = fus.Key.PartitionKey,
                       RowKey = fus.Key.PartitionKey + "_" + fus.Key.SubscriptionId + "_" + fus.Key.MeterId,
                       Quantity = fus.Sum(x => x.Quantity),
                       Name = fus.FirstOrDefault().Name,
                       Type = fus.FirstOrDefault().Type,
                       SubscriptionName = fus.Key.SubscriptionName,
                       SubscriptionId = fus.FirstOrDefault().SubscriptionId,
                       MeterId = fus.Key.MeterId,
                       Unit = fus.Key.Unit,
                       MeterName = fus.Key.MeterName,
                       MeterCategory = fus.Key.MeterCategory,
                       MeterSubCategory = fus.Key.MeterSubCategory,
                       MeterRegion = fus.FirstOrDefault().MeterRegion,
                       DetailsDateTime = fus.FirstOrDefault().DetailsDateTime,
                       PullId = fus.FirstOrDefault().PullId,
                       Amount = GetAmount(fus.Sum(x => x.Quantity), fus.Key.MeterId, meterRateDictionary)
                   };
        }

        private static IEnumerable<AzureUsageDetailsDaily> AggregateAtDailyUsage(List<AzureUsageDetails> usageDetails, Dictionary<string, MeterData> meterRateDictionary)
        {
            return from us in usageDetails
                   group us by new
                   {
                       us.MeterId,
                       us.MeterName,
                       us.MeterCategory,
                       us.MeterSubCategory,
                       us.UsageStartTime,
                       us.SubscriptionId,
                       us.SubscriptionName,
                       us.Unit
                   }
                    into fus
                   select new AzureUsageDetailsDaily()
                   {
                       PartitionKey = GetMonthBasedPartitionKey(fus.Key.UsageStartTime),
                       RowKey = GetInstanceBasedRowKey(fus.Key.SubscriptionId, fus.Key.MeterId, "", fus.Key.UsageStartTime.ToString()),
                       Quantity = fus.Sum(x => x.Quantity),
                       Amount = GetAmount(fus.Sum(x=>x.Quantity),fus.Key.MeterId, meterRateDictionary),
                       Name = fus.FirstOrDefault().Name,
                       Type = fus.FirstOrDefault().Type,
                       SubscriptionId = fus.Key.SubscriptionId,
                       SubscriptionName = fus.Key.SubscriptionName,
                       MeterId = fus.Key.MeterId,
                       UsageStartTime = fus.FirstOrDefault().UsageStartTime,
                       UsageEndTime = fus.FirstOrDefault().UsageEndTime,
                       Unit = fus.Key.Unit,
                       MeterName = fus.Key.MeterName,
                       MeterCategory = fus.Key.MeterCategory,
                       MeterSubCategory = fus.Key.MeterSubCategory,
                       MeterRegion = fus.FirstOrDefault().MeterRegion,
                       DetailsDateTime = fus.FirstOrDefault().DetailsDateTime,
                       PullId = fus.FirstOrDefault().PullId
                   };
        }

        private static Dictionary<string, MeterData> GetMeterData(string subscriptionId, string organizationId, string offerId, string currency, string language, string regionInfo, string baseUrl)
        {
            string requesturl = String.Format("{0}/ratecard/GetBillingData?subscriptionId={1}&organizationId={2}&offerId={3}&currency={4}&language={5}&regionInfo={6}",
                           baseUrl, subscriptionId, organizationId, offerId, currency, language, regionInfo);

            //Build Request
            DateTime jobStartTime = DateTime.UtcNow;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requesturl);

            // Read Response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            var jsonString = reader.ReadToEnd();
            //Get Run Id
            JToken outer = JToken.Parse(jsonString);
            var meters = (JArray)outer["Meters"];
            var meterRateDictionary = new Dictionary<string, MeterData>();
            foreach (var item in meters)
            {
                var meterId = (string)item["MeterId"];
                var meterRates = (JObject)item["MeterRates"];
                var IncludedQuantity = (double)item["IncludedQuantity"];
                if (!meterRateDictionary.ContainsKey(meterId))
                {
                    var meterData = new MeterData { MeterId = meterId, IncludedQuantity = IncludedQuantity };
                    meterData.TieredRates = new Dictionary<double, double>();
                    foreach (var property in meterRates)
                    {
                        meterData.TieredRates.Add(double.Parse(property.Key), double.Parse((string)property.Value));
                    }

                    meterRateDictionary.Add(meterData.MeterId, meterData);
                }
            }

            return meterRateDictionary;
        }

        private static double GetAmountForMeter(double quantity, double included, Dictionary<double, double> rateList)
        {
            var sortedRateList = rateList.Keys.ToList();
            sortedRateList.Sort();
            var sortedTier = sortedRateList.ToArray();
            var amount = 0.0;
            quantity -= included;
            for (int i = 0; i < sortedTier.Length; i++)
            {
                var currentTier = sortedTier[i];
                var tierLimit = GetTieredLimit(sortedTier, i);
                if (quantity >= tierLimit)
                {
                    quantity = quantity - tierLimit;
                    amount += tierLimit * rateList[currentTier];
                }
                else
                {
                    amount += quantity * rateList[currentTier];
                    break;
                }
            }
            return amount;
        }

        private static double GetAmount(double quantity, string meterId, Dictionary<string, MeterData> meterRateDictionary)
        {
            double amount = 0.0;
            if (meterRateDictionary.ContainsKey(meterId))
            {
                amount = GetAmountForMeter(quantity, meterRateDictionary[meterId].IncludedQuantity, meterRateDictionary[meterId].TieredRates);
            }
            return amount;
        }

        private static double GetTieredLimit(double[] sortedRateListArray, int i)
        {
            if (sortedRateListArray.Length > i + 1)
            {
                return sortedRateListArray[i + 1] - sortedRateListArray[i];
            }
            return Double.MaxValue;
        }

        private static string GetMonthBasedPartitionKey(DateTime date)
        {
            var month = date.Month.ToString();
            var year = date.Year.ToString();

            //yyyy-mm format
            return string.Format("{0}-{1}", year, date.Month < 10 ? "0" + month : month);
        }

        private static string GetInstanceBasedRowKey(string subscriptionKey, string meterKey, string instanceKey, string aggregateKey)
        {
            return string.Format("{0}_{1}_{2}_{3}", subscriptionKey, meterKey, instanceKey, aggregateKey).Replace("/", "_").Replace("\\", "_").Replace("#", "_").Replace("?", "_");
        }
    }
}

