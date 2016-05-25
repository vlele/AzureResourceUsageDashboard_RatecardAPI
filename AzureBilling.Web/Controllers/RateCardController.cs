using AzureBilling.Data;
using System.Web.Mvc;

namespace AzureBilling.Web.Controllers
{
    public class RateCardController : Controller
    {
        public ActionResult GetUsageData(string subscriptionId, string organizationId,string startDate,string endDate)
        {
            try
            {
                // get subscription detail from the table storage
                var jsonString = AzureResourceManagerUtil.GetResourceUsageData(subscriptionId, organizationId, startDate, endDate);
                return Content(jsonString, "application/json");

            }
            catch (System.Exception exp)
            {
                Logger.Log("RateCard-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500);
            }

        }

        public ActionResult GetBillingData(string subscriptionId, string organizationId, string offerId, string currency, string language, string regionInfo)
        {
            try
            {
                // get subscription detail from the table storage
                var jsonString = AzureResourceManagerUtil.GetRateCardData(subscriptionId, organizationId, offerId, currency, language, regionInfo);
                return Content(jsonString, "application/json");
            }
            catch (System.Exception exp)
            {
                Logger.Log("RateCard-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500);
            }
        }
    }
}