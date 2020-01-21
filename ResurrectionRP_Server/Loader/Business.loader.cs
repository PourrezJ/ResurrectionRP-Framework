using AltV.Net;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace ResurrectionRP_Server.Loader
{
    public static class BusinessesManager
    {
        public static List<Business.Business> BusinessesList = new List<Business.Business>();
        public static void LoadAllBusinesses()
        {
            Alt.Server.LogColored("--- Start loading all businesses in database ---");

            var _businessesList = Database.MongoDB.GetCollectionSafe<Business.Business>("businesses").AsQueryable();

            foreach (var _businesses in _businessesList)
                _businesses.Init();

            Utils.Util.SetInterval(async () =>
            {
                foreach (var _businesses in _businessesList)
                {
                    _businesses.UpdateInBackground();
                    await Task.Delay(50);
                }
            }, (int)TimeSpan.FromMinutes(5).TotalMilliseconds);

            Alt.Server.LogColored($"--- Finish loading all businesses in database: {_businessesList.Count()} ---");
        }

    }
}
