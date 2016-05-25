using AzureBilling.Data;
using AzureBilling.Web;
using AzureBilling.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureBillingAPI.Web.Controllers
{
    public class SubscriptionController : Controller
    {
        public ActionResult GetSubscriptionDetails()
        {
            try
            {
                string subscriptionJson = ConfigurationManager.AppSettings["Subscriptions"].ToString();
                var subscriptions = JsonConvert.DeserializeObject<UserSubscription>(subscriptionJson);

                return Json(subscriptions, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                Logger.Log("Subscription-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500, exp.Message);
            }
        }
    }
}