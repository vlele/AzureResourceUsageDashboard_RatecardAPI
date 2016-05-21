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

        public ActionResult GetBillingData(string subscriptionId= "ea2939ff-05e6-4818-9a45-6cb7a2902695", string organizationId= "600d2dee-f5ea-4019-b95b-4dfb587455e0", string offerId= "MS-AZR-0063P", string currency="USD", string language="en-US", string regionInfo="IN")
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