using AzureBilling.Data;
using AzureBillingAPI.Data;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureBilling.Web
{
    public class ADTokenCache : TokenCache
    {
        string tokenOwner;
        UserTokenCache tokenCache;
        
        public ADTokenCache(string user)
        {
            // associate the cache to the current user
            tokenOwner = user;
            
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;

            // look up the entry in the storage
            tokenCache = GetCacheFromStorage(tokenOwner);
            
            // place the entry in memory
            this.Deserialize((tokenCache == null) ? null : tokenCache.CacheBits);
        }

        private UserTokenCache GetCacheFromStorage(string user)
        {
            EntityRepo<UserTokenCache> repo = new EntityRepo<UserTokenCache>();
            var list = repo.Get("UserTokenCache",null,user);
            var first = list.FirstOrDefault();
            return first;
        }

        // Before Token Access event handler
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            if (tokenCache == null)
            {
                // first time access
                tokenCache = GetCacheFromStorage( tokenOwner);
            }
            else
            {   
                // if the in-memory copy is older than the persistent copy
                if (tokenCache.LastWrite > tokenCache.LastWrite)
                //// read from from storage, update in-memory copy
                {
                    tokenCache = GetCacheFromStorage(tokenOwner);
                }
            }
            this.Deserialize((tokenCache == null) ? null : tokenCache.CacheBits);
        }
        
        // After Token Access EventHandler
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if state changed
            if (this.HasStateChanged)
            {
                // check for an existing entry
                tokenCache = GetCacheFromStorage(tokenOwner);
                if (tokenCache == null)
                {
                    // if no existing entry for that user, create a new one
                    tokenCache = new UserTokenCache
                    {
                        WebUserUniqueId = tokenOwner,
                    };
                }

                // update the cache contents and the last write timestamp
                tokenCache.CacheBits = this.Serialize();
                tokenCache.LastWrite = DateTime.UtcNow;

                // update the storage with modification or new entry
                WriteUserTokenCache(tokenCache);
                this.HasStateChanged = false;
            }
        }

        private void WriteUserTokenCache(UserTokenCache cache)
        {
            EntityRepo<UserTokenCache> repo = new EntityRepo<UserTokenCache>();
            repo.Insert(new List<UserTokenCache> { cache });
        }

        // clean up the storage
        public override void Clear()
        {
            base.Clear();
            EntityRepo<UserTokenCache> repo = new EntityRepo<UserTokenCache>();
            repo.Delete(new UserTokenCache { RowKey = tokenOwner });
        }
    }
}