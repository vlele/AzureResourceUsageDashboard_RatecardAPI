using AzureBilling.Data;
using AzureBilling.Web;
using AzureBilling.Web.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AzureBillingAPI.Web.Controllers
{
    public class SubscriptionController : Controller
    {

        private const string USER_SUBSCRIPTION_TABLE_PARTITION_KEY = "UserSubscription";
        public ActionResult Refresh()
        {
            try
            {
                // get all organizations and their respective subscription for a given tenant
                var orgs = AzureResourceManagerUtil.GetUserOrganizations();
                Dictionary<Organization, List<Subscription>> dictionary = new Dictionary<Organization, List<Subscription>>();
                foreach (var item in orgs)
                {
                    if (!dictionary.ContainsKey(item))
                    {
                        var subscriptions = AzureResourceManagerUtil.GetUserSubscriptions(item.Id);
                        dictionary.Add(item, subscriptions);
                    }
                }

                // check if these subscriptions are already added in the storage
                var repo = new EntityRepo<UserSubscription>();
                var list = repo.Get(USER_SUBSCRIPTION_TABLE_PARTITION_KEY, null, "");
                var existingSubscriptions = new Dictionary<string, string>();
                foreach (var item in list)
                {
                    existingSubscriptions.Add(item.SubscriptionId, item.SubscriptionId);
                }

                // list of new subscription to add
                var listOfUserSubscription = new List<UserSubscription>();
                foreach (var subscriptions in dictionary.Values)
                {
                    foreach (var subscription in subscriptions)
                    {
                        UserSubscription userSubscription = new UserSubscription(subscription.Id, subscription.OrganizationId);
                        userSubscription.DisplayName = subscription.DisplayName;

                        // if the subscription is not already in the storage add them
                        // otherwise the one in the storage should have latest info
                        if (!existingSubscriptions.ContainsKey(userSubscription.SubscriptionId))
                        {
                            listOfUserSubscription.Add(userSubscription);
                        }
                    }
                }

                // if one or more subscriptions are discovered, add them
                if (listOfUserSubscription.Count > 0)
                {
                    repo.Insert(listOfUserSubscription);
                }
                return Json(dictionary.ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                Logger.Log("Subscription-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500, exp.Message);
            }
            
        }

        [HttpPost]
        public ActionResult SaveSubscriptionInfo(List<UserSubscription> subscriptions)
        {
            try
            {
                var listOfUserSubscription = new List<UserSubscription>();
                foreach (var subscription in subscriptions)
                {
                    UserSubscription userSubscription = new UserSubscription(subscription.SubscriptionId, subscription.OrganizationId);
                    userSubscription.DisplayName = subscription.DisplayName;
                    userSubscription.OfferId = subscription.OfferId;
                    userSubscription.Currency = subscription.Currency;
                    userSubscription.RegionInfo = subscription.RegionInfo;
                    listOfUserSubscription.Add(userSubscription);
                }

                var repo = new EntityRepo<UserSubscription>();
                if (listOfUserSubscription.Count > 0)
                {
                    repo.Insert(listOfUserSubscription);
                }
                return Json(new { Message = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                Logger.Log("Subscription-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500, exp.Message);
            }
            
        }

        public ActionResult GetSubscriptionDetails()
        {
            try
            {
                EntityRepo<UserSubscription> subscriptionRepo = new EntityRepo<UserSubscription>();
                var list = subscriptionRepo.Get(USER_SUBSCRIPTION_TABLE_PARTITION_KEY, null, "").Select(p => new {
                    SubscriptionId = p.SubscriptionId,
                    OrganizationId = p.OrganizationId,
                    DisplayName = p.DisplayName,
                    OfferId = p.OfferId,
                    Currency = p.Currency,
                    RegionInfo = p.RegionInfo
                }); ;

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exp)
            {
                Logger.Log("Subscription-Web-API", "Error", exp.Message, exp.ToString());
                return new HttpStatusCodeResult(500, exp.Message);

            }

        }

    }
}