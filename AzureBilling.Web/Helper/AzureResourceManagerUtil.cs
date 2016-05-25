using AzureBilling.Web.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Helpers;

namespace AzureBilling.Web
{
    /// <summary>
    /// Helper class that makes calls to Azure Resource Manager API to get Resources information
    /// </summary>
    public static class AzureResourceManagerUtil
    {
        public static string GetRateCardData(string subscriptionId, string organizationId, string offerId, string currency, string language, string regionInfo)
        {
            string usageText = "";
            AuthenticationResult result = GetFreshAuthToken(organizationId);
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["BillingAPIUrlFormat"];

            // Making a call to the Azure Usage API for a set time frame with the input AzureSubID
            string requesturl = String.Format(baseUrl,
                subscriptionId,
                "2015-06-01-preview",
                offerId,
                currency,
                language,
                regionInfo);

            // HTTP call
            usageText = GetJson(result, requesturl);
            var billingData = Json.Decode(usageText);
            var meters = billingData["Meters"];
            return usageText;
        }

        private static string GetJson(AuthenticationResult result, string requesturl)
        {
            string usageText;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requesturl);
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + result.AccessToken);
            request.ContentType = "application/json";

            // Read Response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Console.WriteLine(response.StatusDescription);
            Stream receiveStream = response.GetResponseStream();

            // read stream as text
            StreamReader responseStream = new StreamReader(receiveStream, Encoding.UTF8);
            usageText = responseStream.ReadToEnd();
            return usageText;
        }

        private static AuthenticationResult GetFreshAuthToken(string organizationId)
        {
            // Aquire Access Token to call Azure Resource Manager
            string signedInUserUniqueName = ConfigurationManager.AppSettings["UserID"];
            ClientCredential credential = new ClientCredential(ConfigurationManager.AppSettings["ClientID"],
                ConfigurationManager.AppSettings["ApplicationKey"]);
            AuthenticationContext authContext = new AuthenticationContext(
                string.Format(ConfigurationManager.AppSettings["Authority"], organizationId), new ADTokenCache(signedInUserUniqueName));
            AuthenticationResult result = authContext.AcquireToken(ConfigurationManager.AppSettings["AzureResourceManagerIdentifier"], credential);
            return result;
        }

        public static string GetResourceUsageData(string subscriptionId, string organizationId, string startTime, string endTime)
        {
            string usageText = "";
            string baseUrl = ConfigurationManager.AppSettings["UsageAPIUrlFormat"];
            
            // get token
            AuthenticationResult result = GetFreshAuthToken(organizationId);

            // Making a call to the Azure Usage API for a set time frame with the input AzureSubID
            string requesturl = String.Format(baseUrl,
                subscriptionId,
                startTime,
                endTime);

            // HTTP call
            usageText = GetJson(result, requesturl);

            return usageText;
        }
    }
}